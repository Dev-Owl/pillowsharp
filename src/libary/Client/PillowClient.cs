
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

namespace PillowSharp.Client
{
    public class PillowClient
    {
        //TODO If list grows move to seperated class to reduce size here
        //List of defined entry points in couchDB
        public const string AllDBQuery = "_all_dbs";
        public const string BulkOperation = "_bulk_docs";
        public const string Session = "_session";
        public const string CoockieName = "AuthSession";
        public const string NewUUID = "_uuids";
        
        public const string ParamCount = "count";

        //Information about server,user and authentication method
        public ICouchdbServer ServerHelper { get; set; } 
        
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
            JSONHelper = new BasicJSONHelper();
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

        public async Task Authenticate()
        {
            //Only run this if auth type is set to token
            if(ServerHelper.LoginType == ELoginTypes.TokenLogin){
                //Try to get token from storage for current user
                var loginData = ServerHelper.GetLoginData();
                var token = tokenStorage.Get(loginData.UserName);
                if(string.IsNullOrEmpty( token)){
                    //new token is required post to server
                    var response = await RequestHelper.Post(Session,JSONHelper.ToJSON(loginData));
                    //parese server response
                    var loginResponse = JSONHelper.FromJSON<CouchLoginResponse>(response);
                    //thanks to the shared coockie storage all request will now have this coockie active
                    //in case ok store token in storage
                    if(loginResponse.ok){
                        //store new token
                        token = response.Cookies.FirstOrDefault(c => c.Name ==CoockieName)?.Value;
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
                    RequestHelper.SetCoockie(CoockieName,token);
                }
            }
        }
        //Get IDs ,UUID, from couchDB by default one 
        public async Task<CouchUUIDResponse> GetUUID(int Count=1)
        {
            //Make the request and return the list of IDS 
            return JSONHelper.FromJSON<CouchUUIDResponse>( await RequestHelper.Get(NewUUID,new KeyValuePair<string, object>(ParamCount,Count)));
        }
       
        //Internal, convert all response to the given object
        private T FromResponse<T>(IRestResponse Response) where T:new()
        {
             return JSONHelper.FromJSON<T>(Response);
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
            var result = await RequestHelper.Get(AllDBQuery);
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
            return FromResponse<CouchConfirm>(result)?.ok ?? false;
        }

        //Create a document in the given server and database
        public async Task<CouchDocumentChange> CreateDocument<T>(T NewDocument,string Database=null) where T:new()
        {
            //Get datbase to use
            Database = GetDB(typeof(T),Database);
            //Create token if needed
            await Authenticate();
            //Request and build json for T
            var result = FromResponse<CouchDocumentChange>( await Post(Database,JSONHelper.ToJSON(NewDocument)));
            //If result is ok and the new created entity is based on CouchDocument set values as returned by the server
            if(result.ok && NewDocument is CouchDocument)
            {
                var couchDoc = (NewDocument as CouchDocument);
                couchDoc._id = result.id;
                couchDoc._rev = result.rev;
                couchDoc._deleted = false;
            }
            return result;
        }
        
        public async Task<CouchDocumentChange> DeleteDocument<T>(T Document,string Database=null) where T:CouchDocument
        {
            //Get datbase to use
            Database = GetDB(typeof(T),Database);
            //Create token if needed
            await Authenticate();
            //Ensure the document is marked as deleted
            Document._deleted=true;
            //As this is a single delete we still use the bulk operation that returns an array
            return FromResponse<List<CouchDocumentChange>>( await Post($"{Database}/{BulkOperation}",JSONHelper.ToJSON(new CouchBulk(Document))))[0];
        }

        //Shorthand fucntion for simple post request
        private Task<IRestResponse> Post(string URL,string Body)
        {
            return RequestHelper.Post(URL,Body);
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
        //Get a single document, optional with rev number
        public async Task<T> GetDocument<T>(string ID,string Revision=null,string Database=null) where T : new()
        {
            //Document == null -> check if entity has meta 
            Database = GetDB(typeof(T),Database);
            //Create token if needed
            await Authenticate();
            var result = await RequestHelper.GetDocument(ID,Database,Revision:Revision);
            return FromResponse<T>(result);
        }
        //Get the DB to use
        //Priority list:
        // 1. Caller Parameter
        // 2. Class DB, see Database Property
        // 3. DBNameAttribute of the current document
        private string GetDB(Type T,string Database)
        {
            //If DB provided by caller nothing to do
            if(string.IsNullOrEmpty(Database)){
                //Override with class DB if set
                Database = this.Database;
                if(string.IsNullOrEmpty(Database) && T != null)
                {
                    //Last try check if document type has DBNameAttribute set
                    Database = (T.GetTypeInfo().GetCustomAttribute(typeof(DBNameAttribute))as DBNameAttribute)?.Name;
                }
                //Fail if no DB was found
                if(string.IsNullOrEmpty(Database))
                    throw new PillowException("No database set, unable to request");
            }
            //Return the db that was found
            return Database;
        }


        

    }
}