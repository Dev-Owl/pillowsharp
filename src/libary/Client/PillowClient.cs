
using PillowSharp.Middelware;
using PillowSharp.Middelware.Default;
using PillowSharp.CouchType;
using PillowSharp.BaseObject;


using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using RestSharp;
using System.Linq;
using System.IO;
using PillowSharp.Helper;
using System.Net;

namespace PillowSharp.Client
{
    public class PillowClient
    {
        //Settings
        public PillowClientSettings Settings = new  PillowClientSettings();

        private ICouchdbServer _ServerHelper = null;
        //Information about server,user and authentication method
        public ICouchdbServer ServerHelper 
        { 
            get{
                return _ServerHelper;
            }
            set{
                _ServerHelper = value;
                RequestHelper?.UpdateServerData(value);
            }
        } 
        
        //Class to handle all web request with couchdb
        public AWebRequestHelper RequestHelper {get; set;}
        
        //JSON conversion for all response and requests
        public IJSONHelper    JSONHelper { get; set; }

        //Force this database during all requests
        public string Database { get; set; }

        //Storage for token shared between instances
        private static TokenStorage tokenStorage = new TokenStorage();


        //Create client with default middleware just the server config is needed
        public PillowClient(BasicCouchDBServer Server)
        {
            ServerHelper = Server;
            RequestHelper = new BasicWebRequestHelper(ServerHelper);
            JSONHelper = new BasicJSONHelper(Settings.IgnoreJSONNull);
        }
        //Custom client allows to set all parts
        public PillowClient(ICouchdbServer ServerHelper,
                            AWebRequestHelper RequestHelper,
                            IJSONHelper    JSONHelper)
        {
            this.ServerHelper  = ServerHelper;
            this.RequestHelper = RequestHelper;
            this.JSONHelper = JSONHelper;
        }

#region Server/DB Functions
        public async Task Authenticate()
        {
            //Only run this if auth type is set to token
            if(ServerHelper.LoginType == ELoginTypes.TokenLogin){
                //Try to get token from storage for current user
                var loginData = ServerHelper.GetLoginData();
                var token = tokenStorage.Get(loginData.UserName);
                if(string.IsNullOrEmpty( token)){
                    //new token is required post to server
                    var response = await RequestHelper.Post(CouchEntryPoints.Session,JSONHelper.ToJSON(loginData));
                    //parese server response
                    var loginResponse = JSONHelper.FromJSON<CouchLoginResponse>(response);
                    //thanks to the shared coockie storage all request will now have this coockie active
                    //in case ok store token in storage
                    if(loginResponse.Ok){
                        //store new token
                        token = response.Cookies.FirstOrDefault(c => c.Name == CouchEntryPoints.CoockieName)?.Value;
                        //Ensure that token exists
                        if(!string.IsNullOrEmpty(token))
                            tokenStorage.Add(loginData.UserName,token);
                        else
                            throw new PillowException("Authentication coockie not found");
                    }
                    else{
                        throw new PillowException("Response from Session call was negative");
                    }
                }
                else{
                    //Ensure that coockie is set for all requests, can be a new instance not yet made a call
                    RequestHelper.SetCoockie(CouchEntryPoints.CoockieName,token);
                }
            }
        }

        //Get IDs ,UUID, from couchDB by default one 
        public async Task<CouchUUIDResponse> GetUUID(int Count=1)
        {
            //Make the request and return the list of IDS 
            return JSONHelper.FromJSON<CouchUUIDResponse>( await RequestHelper.Get(CouchEntryPoints.NewUUID,new KeyValuePair<string, object>(CouchEntryPoints.ParamCount,Count)));
        }
       
        //Check if a db exists in the current server
        public async Task<bool> DbExists(string Name)
        {
            //Get list of all db and check if included
            return (await AllDatabase()).Contains(Name);
        }
        
        //Get a list of all current dbs
        public async Task<List<string>> AllDatabase()
        {
            //Create token if needed
            await Authenticate();
            //Make the request to get the list of db
            var result = await RequestHelper.Get(CouchEntryPoints.AllDBQuery);
            //return result
            return FromResponse<List<string>>(result);
        }

        //Create a database on the server with the given name
        public async Task<bool> CreateDatabase(string Name)
        {
            //Create token if needed
            await Authenticate();
            //Create the db via a put call
            var result = await RequestHelper.Put(Name);
            //Return response state
            return FromResponse<CouchConfirm>(result)?.Ok ?? false;
        }

        public async Task<bool> DeleteDatbase(string Name)
        {
            return JSONHelper.FromJSON<CouchConfirm>(await RequestHelper.Delete(Name))?.Ok ?? false;
        }
#endregion

#region Document Functions
        //Create a document in the given server and database
        public async Task<CouchDocumentChange> CreateDocument<T>(T NewDocument,string Database=null) where T:new()
        {
            //Get datbase to use
            Database = GetDB(typeof(T),Database);
            //Create token if needed
            await Authenticate();

            if (NewDocument is CouchDocument)
            {
                
                var couchDoc = NewDocument as CouchDocument;
                if (string.IsNullOrEmpty(couchDoc.ID))
                {
                    if(Settings.AutoGenerateID)
                        if(Settings.UseCouchUUID)
                            couchDoc.ID = (await GetUUID()).UUIDS[0];
                        else
                            couchDoc.ID = Guid.NewGuid().ToString();
                }
            }
            //Request and build json for T
            var result = FromResponse<CouchDocumentChange>( await Post(Database,JSONHelper.ToJSON(NewDocument)));
            //If result is ok and the new created entity is based on CouchDocument set values as returned by the server
            if(result.Ok && NewDocument is CouchDocument)
            {
                var couchDoc = (NewDocument as CouchDocument);
                couchDoc.ID = result.ID;
                couchDoc.Rev = result.Rev;
                couchDoc.Deleted = false;
            }
            return result;
        }
        
        //Delete a single document in the db
        public async Task<CouchDocumentChange> DeleteDocument<T>(T Document,string Database=null) where T:CouchDocument
        {
            return (await DeleteDocument<T>(new List<T>(){Document},Database))[0];
        }

        //Delete a list of documents in the db
        public async Task<List<CouchDocumentChange>> DeleteDocument<T>(List<T> Documents,string Database=null) where T : CouchDocument
        {
            //Get datbase to use
            Database = GetDB(typeof(T),Database);
            //Create token if needed
            await Authenticate();
            //Ensure the document is marked as deleted
            Documents.ForEach(d => d.Deleted=true);
            //Use bulk operation to delete the list of documents
            var responseList = FromResponse<List<CouchDocumentChange>>( await Post($"{Database}/{CouchEntryPoints.BulkOperation}",JSONHelper.ToJSON(new CouchBulk<T>(Documents)))); //TODO Ugly, rewrite
            //Update Revision numbers for caller
            responseList.ForEach(response => {if(response.Ok) Documents.First(d => d.ID == response.ID).Rev = response.Rev;});
            return responseList;
        }

        public async Task<CouchDocumentChange> UpdateDocument<T>(T Document,string Database = null) where T : CouchDocument
        {
            //Call bulk update db will be set in there
            return (await UpdateDocument<T>(new List<T>(){Document},Database))[0];
        }
        //Update,Create,Delete Documents
        public async Task<List<CouchDocumentChange>> UpdateDocument<T>(List<T> Documents,string Database = null) where T : CouchDocument
        {
            Database = GetDB(typeof(T), Database);
            //Ensure IDS are created if setting is active
            if(Settings.AutoGenerateID){
               var missingIDs = Documents.Where(d =>string.IsNullOrEmpty(d.ID)).ToList();
               if(missingIDs.Count > 0){
                    List<string> ids = null; 
                    //Swith between CouchID and GUID 
                    if(Settings.UseCouchUUID)
                        ids = (await GetUUID(missingIDs.Count)).UUIDS;
                    else
                        ids = GetGUID(missingIDs.Count);
                    for(var i=0;i < missingIDs.Count;++i)
                    {
                        missingIDs[i].ID = ids[i];
                    }
               }
               
            }
            //Use bulk to update list of documents in given db
            var result = FromResponse<List<CouchDocumentChange>>( await Post($"{Database}/{CouchEntryPoints.BulkOperation}",JSONHelper.ToJSON(new CouchBulk<T>(Documents)))); //TODO Ugly, rewrite
            //Set new Revisions
            result.Where(cc => cc.Ok).ToList().ForEach(cc => 
                {
                    var current_doc = Documents.FirstOrDefault(d => d.ID == cc.ID);
                    if(current_doc != null)
                        current_doc.Rev = cc.Rev;
                }
            );
           

            return result;
        }
        
        //Basic function to get a list of documents, allows the usage of views or any multi response URL
        public async Task<CouchDocumentResponse<T>> GetDocuments<T>(string RequestURL) where T: new() 
        {
           //Create token if needed
           await Authenticate();
           //Get request for given URL, no db magic
           var result = await RequestHelper.Get(RequestURL);
           return FromResponse<CouchDocumentResponse<T>>(result);
        }

        //Get a single document, optional with Rev number
        public async Task<T> GetDocument<T>(string ID,string Revision=null,string Database=null) where T : new()
        {
            //Document == null -> check if entity has meta 
            Database = GetDB(typeof(T),Database);
            //Create token if needed
            await Authenticate();
            var result = await RequestHelper.GetDocument(ID,Database,Revision:Revision);
            return FromResponse<T>(result);
        }

        //Get all documents in the given database
        public async Task<CouchDocumentResponse<CouchViewResponse<AllDocResponse>>> GetAllDocuments(Type Document = null,string Database=null) 
        {
            Database = GetDB(Document,Database); // Get database to use
            return JSONHelper.FromJSON<CouchDocumentResponse<CouchViewResponse<AllDocResponse>>>( await RequestHelper.Get($"{Database}/{CouchEntryPoints.AllDocs}")); //return result to client
        }
        //TODO add a function that allows to pass a stream and mime type to use
        public async Task<CouchDocumentChange> AddAttachment<T>(T Document,string AttributeName,string File,string Database=null) where T : CouchDocument
        {
            if(!System.IO.File.Exists(File))
                throw new PillowException($"Unable to find file {File}!");
            Database = GetDB(typeof(T),Database);
            var result = JSONHelper.FromJSON<CouchDocumentChange>( await RequestHelper.UploadFile(Document.ID,AttributeName,Document.Rev,Database,File));
            if(result.Ok){
                Document.Rev = result.Rev; // Update Revision number for caller
            }
            return result;
        }

        public async Task<byte[]> GetAttachement<T>(T Document,string AttributeName,string Database=null) where T : CouchDocument
        {
            Database = GetDB(typeof(T),Database);
            //Ask for file data
            var response = await RequestHelper.GetFile(Document.ID,AttributeName,Document.Rev,Database);
            //In case something went wrong throw an error
            if(response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                PillowErrorHelper.HandleNoneOKResponse(response,JSONHelper);
            //Return file data
            return response.RawBytes;
        }

        public async Task<CouchDocumentChange> DeleteAttachment<T>(T Document,string AttributeName,string Database=null) where T : CouchDocument
        {
            Database = GetDB(typeof(T),Database);
            var result = JSONHelper.FromJSON<CouchDocumentChange>( await RequestHelper.DeleteFile(Document.ID,AttributeName,Document.Rev,Database));
            if(result.Ok){
                Document.Rev = result.Rev;
            }
            return result;
        }

        public async Task<CouchDocumentResponse<T>> GetView<T>(string DocumentID,string ViewFunctionName,KeyValuePair<string,object>[] QueryParameter=null,string Database =null) where T:new()
        {
            Database = GetDB(typeof(T),Database);
            if(!DocumentID.StartsWith(CouchEntryPoints.DesignDoc)) 
                DocumentID = $"{CouchEntryPoints.DesignDoc}/{DocumentID}";
            return JSONHelper.FromJSON<CouchDocumentResponse<T>>(await RequestHelper.View(Database,DocumentID,ViewFunctionName,QueryParameter,Method.GET,null));
        }

        public async Task<CouchDocumentResponse<T>> FilterView<T>(string DocumentID,string ViewFunctionName,CouchViewFilter ViewFilter,KeyValuePair<string,object>[] QueryParameter=null,string Database =null) where T:new()
        {
            Database = GetDB(typeof(T),Database);
            if(!DocumentID.StartsWith(CouchEntryPoints.DesignDoc)) 
                DocumentID = $"{CouchEntryPoints.DesignDoc}/{DocumentID}";
            return JSONHelper.FromJSON<CouchDocumentResponse<T>>(await RequestHelper.View(Database,DocumentID,ViewFunctionName,QueryParameter,Method.POST,JSONHelper.ToJSON(ViewFilter)));
        }
        
        public async Task<CouchDesignDocument> GetDesignDocument(string DocumentID, string Database=null)
        {
            Database = GetDB(null,Database);           
             if(!DocumentID.StartsWith(CouchEntryPoints.DesignDoc)) 
                DocumentID = $"{CouchEntryPoints.DesignDoc}/{DocumentID}";
                                         //TODO No URL building, add helper function in RequestHelper
            return JSONHelper.FromJSON<CouchDesignDocument>(await RequestHelper.Get($"{Database}/{DocumentID}"));
        }

        public async Task<CouchDocumentChange> UpsertDesignDocument(CouchDesignDocument DesignDocument,string Database =null)
        {
             Database = GetDB(DesignDocument.GetType(),Database);  
             if (string.IsNullOrEmpty(DesignDocument.ID))
             {
                 if(Settings.AutoGenerateID)
                     if(Settings.UseCouchUUID)
                         DesignDocument.ID = (await GetUUID()).UUIDS[0];
                     else
                         DesignDocument.ID = Guid.NewGuid().ToString();
             }       
             if(!DesignDocument.ID.StartsWith(CouchEntryPoints.DesignDoc)) 
                DesignDocument.ID = $"{CouchEntryPoints.DesignDoc}/{DesignDocument.ID}";
                                                                               //TODO No URL building, add helper function in RequestHelper
             var result = JSONHelper.FromJSON<CouchDocumentChange>( await RequestHelper.Put($"{Database}/{DesignDocument.ID}",JSONHelper.ToJSON(DesignDocument)));
             if(result.Ok)
             {
                DesignDocument.ID = result.ID;
                DesignDocument.Rev = result.Rev;
             }
             return result;
        }


#endregion

#region Private Functions        
        //Get the DB to use
        //Priority list:
        // 1. Caller Parameter
        // 2. Class DB, see Database Property
        // 3. DBNameAttribute of the current document
        private string GetDB(Type T,string Database)
        {
            //If DB provided by caller nothing to do
            if(string.IsNullOrEmpty(Database)){
 
                if(T != null)
                {
                    //Last try check if document type has DBNameAttribute set
                    Database = (T.GetTypeInfo().GetCustomAttribute(typeof(DBNameAttribute))as DBNameAttribute)?.Name;
                }
                //Override with class DB if set
                if(string.IsNullOrEmpty(Database))
                    Database = this.Database;

                //Fail if no DB was found
                if(string.IsNullOrEmpty(Database))
                    throw new PillowException("No database set, unable to request");
            }
            //Return the db that was found
            return Database;
        }

        //Shorthand fucntion for simple post request
        private Task<IRestResponse> Post(string URL,string Body)
        {
            return RequestHelper.Post(URL,Body);
        }

                //Internal, convert all response to the given object
        private T FromResponse<T>(IRestResponse Response) where T:new()
        {
             return JSONHelper.FromJSON<T>(Response);
        }

        //Create a list of GUIDS as string 
        private List<String> GetGUID(int Amount=1){
            var i = 0;
            var result = new List<string>();
            while(i < Amount){
                result.Add( Guid.NewGuid().ToString());
                i +=1;
            }
            return result;
        }
#endregion       
        
        

    }
}