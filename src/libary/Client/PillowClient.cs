
using pillowsharp.Middleware;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using PillowSharp.Helper;
using PillowSharp.Middleware;
using PillowSharp.Middleware.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace PillowSharp.Client
{
    public class PillowClient
    {
        /// <summary>
        /// Settings for this client
        /// </summary>
        public PillowClientSettings ClientSettings { get; set; } = new PillowClientSettings();

        private ICouchdbServer serverConfiguration = null;

        /// <summary>
        /// Information about server,user and authentication method
        /// </summary>
        public ICouchdbServer ServerConfiguration
        {
            get
            {
                return serverConfiguration;
            }
            set
            {
                serverConfiguration = value;
                RequestHelper?.UpdateServerData(value);
            }
        }

        /// <summary>
        /// Class to handle all web request with couchdb
        /// </summary>
        public AWebRequestHelper RequestHelper { get; set; }

        /// <summary>
        /// JSON conversion for all response and requests
        /// </summary>
        public IJSONHelper JSONHelper { get; set; }

        /// <summary>
        /// Force this database during all requests, overrides all other settings
        /// </summary>
        public string ForcedDatabaseName { get; set; }

        //Storage for token shared between instances
        private TokenStorage loginTokenStorage = new TokenStorage();


        /// <summary>
        /// Create client with default middleware just the server config is needed
        /// </summary>
        /// <param name="ServerConfiguration">Server url, user and password</param>
        public PillowClient(BasicCouchDBServer ServerConfiguration)
        {
            this.ServerConfiguration = ServerConfiguration;
            RequestHelper = new BasicWebRequestHelper(this.ServerConfiguration);
            JSONHelper = new BasicJSONHelper(ClientSettings.IgnoreJSONNull);
        }


        /// <summary>
        /// Custom client allows to set all middleware parts
        /// </summary>
        /// <param name="ServerConfiguration">Server url, user and password</param>
        /// <param name="RequestHelper">Middleware for webrequests</param>
        /// <param name="JSONHelper">Middleware to handle JSON tasks</param>
        public PillowClient(ICouchdbServer ServerConfiguration,
                            AWebRequestHelper RequestHelper,
                            IJSONHelper JSONHelper)
        {
            this.ServerConfiguration = ServerConfiguration;
            this.RequestHelper = RequestHelper;
            this.JSONHelper = JSONHelper;
        }

        //TODO Think about nicer way for await Authenticate in each call

        #region Server/DB Functions
        /// <summary>
        /// If Token authentication is used, login and store the token or add an existing to the current request
        /// </summary>
        /// <returns></returns>
        public async Task Authenticate()
        {
            //Only run this if auth type is set to token
            if (ServerConfiguration.LoginType == ELoginTypes.TokenLogin)
            {
                //Try to get token from storage for current user
                var currentLoginData = ServerConfiguration.GetLoginData();
                var alreadyCreatedLoginToken = loginTokenStorage.Get(currentLoginData.UserName);
                if (string.IsNullOrEmpty(alreadyCreatedLoginToken))
                {
                    //new token is required post to server
                    var restSessionCreateResponse = await RequestHelper.Post(CouchEntryPoints.Session, JSONHelper.ToJSON(currentLoginData));
                    //parese server response
                    var loginResponse = JSONHelper.FromJSON<CouchLoginResponse>(restSessionCreateResponse);
                    //thanks to the shared coockie storage all request will now have this coockie active
                    //in case ok store token in storage
                    if (loginResponse.Ok)
                    {
                        //store new token
                        alreadyCreatedLoginToken = restSessionCreateResponse.Cookies.FirstOrDefault(c => c.Name == CouchEntryPoints.CoockieName)?.Value;
                        //Ensure that token exists
                        if (!string.IsNullOrEmpty(alreadyCreatedLoginToken))
                            loginTokenStorage.Add(currentLoginData.UserName, alreadyCreatedLoginToken);
                        else
                            throw new PillowException("Authentication coockie not found in CouchDB response");
                    }
                    else
                    {
                        throw new PillowException("CouchDB login failed no token generated");
                    }
                }
                else
                {
                    //Ensure that coockie is set for all requests, can be a new instance not yet made a call
                    RequestHelper.SetCookie(CouchEntryPoints.CoockieName, alreadyCreatedLoginToken);
                }
            }
        }

        /// <summary>
        /// Get IDs ,UUID, from couchDB by default one 
        /// </summary>
        /// <returns>New UUID</returns>
        public async Task<string> GetSingleUUID()
        {
            var result = await _GetUUIDs(AmountOfUUIDs: 1);
            return result.UUIDS.FirstOrDefault();
        }

        /// <summary>
        /// Get IDs ,UUID, from couchDB
        /// </summary>
        /// <param name="AmountOfUUIDs">Number of ID´s you need</param>
        /// <returns>List of ID´s</returns>
        public async Task<CouchUUIDResponse> GetManyUUIDs(int AmountOfUUIDs)
        {
            return await _GetUUIDs(AmountOfUUIDs);
        }

        private async Task<CouchUUIDResponse> _GetUUIDs(int AmountOfUUIDs = 1)
        {
            //Create token if needed
            await Authenticate();
            //Make the request and return the list of IDS 
            return JSONHelper.FromJSON<CouchUUIDResponse>(await RequestHelper.Get(CouchEntryPoints.NewUUID, new KeyValuePair<string, object>(CouchEntryPoints.ParamCount, AmountOfUUIDs)));
        }

        /// <summary>
        /// Check if a db exists in the current server
        /// </summary>
        /// <param name="Name">Database name</param>
        /// <returns></returns>
        public async Task<bool> DbExists(string Name)
        {
            //Create token if needed
            await Authenticate();
            //Get list of all db and check if included
            return (await GetListOfAllDatabases()).Contains(Name);
        }

        /// <summary>
        /// Get a list of all current dbs
        /// </summary>
        /// <returns>List of db names</returns>
        public async Task<List<string>> GetListOfAllDatabases()
        {
            //Create token if needed
            await Authenticate();
            //Make the request to get the list of db
            var result = await RequestHelper.Get(CouchEntryPoints.AllDBQuery);
            //return result
            return FromResponse<List<string>>(result);
        }

        /// <summary>
        /// Create a database on the server with the given name
        /// </summary>
        /// <param name="Name">New database name</param>
        /// <returns></returns>
        public async Task<bool> CreateNewDatabase(string Name)
        {
            //Create token if needed
            await Authenticate();
            //Create the db via a put call
            var result = await RequestHelper.Put(Name);
            //Return response state
            return FromResponse<CouchConfirm>(result)?.Ok ?? false;
        }

        /// <summary>
        /// Delete a database on the server
        /// </summary>
        /// <param name="Name">Name of the database</param>
        /// <returns></returns>
        public async Task<bool> DeleteDatbase(string Name)
        {
            await Authenticate();
            return JSONHelper.FromJSON<CouchConfirm>(await RequestHelper.Delete(Name))?.Ok ?? false;
        }
        #endregion

        #region Document Functions
        /// <summary>
        /// Create a document in the given server and database
        /// </summary>
        /// <typeparam name="T">Type of the document</typeparam>
        /// <param name="NewDocument">The new document</param>
        /// <param name="DatabaseToCreateDocumentIn">Optional, set the db name</param>
        /// <returns>The change record from CouchDb</returns>
        public async Task<CouchDocumentChange> CreateANewDocument<T>(T NewDocument, string DatabaseToCreateDocumentIn = null) where T : new()
        {
            //Get datbase to use
            DatabaseToCreateDocumentIn = GetDBToUseForRequest(typeof(T), DatabaseToCreateDocumentIn);
            //Create token if needed
            await Authenticate();

            if (NewDocument is CouchDocument)
            {
                var couchDoc = NewDocument as CouchDocument;
                if (string.IsNullOrEmpty(couchDoc.ID))
                {
                    if (ClientSettings.AutoGenerateID)
                        if (ClientSettings.UseCouchUUID)
                            couchDoc.ID = await GetSingleUUID();
                        else
                            couchDoc.ID = Guid.NewGuid().ToString();
                }
            }
            //Request and build json for T
            var result = FromResponse<CouchDocumentChange>(await HttpPost(DatabaseToCreateDocumentIn, JSONHelper.ToJSON(NewDocument)));
            //If result is ok and the new created entity is based on CouchDocument set values as returned by the server
            if (result.Ok && NewDocument is CouchDocument)
            {
                var couchDoc = (NewDocument as CouchDocument);
                couchDoc.ID = result.ID;
                couchDoc.Rev = result.Rev;
                couchDoc.Deleted = false;
            }
            return result;
        }

        /// <summary>
        /// Delete a single document in the db
        /// </summary>
        /// <typeparam name="T">Type of the document</typeparam>
        /// <param name="Document">Document to delete</param>
        /// <param name="DatabaseToDeleteDocumentIn">Optional, set the db name</param>
        /// <returns>The change record from CouchDb</returns>
        public async Task<CouchDocumentChange> DeleteDocument<T>(T Document, string DatabaseToDeleteDocumentIn = null) where T : CouchDocument
        {
            return (await DeleteDocuments<T>(new List<T>() { Document }, DatabaseToDeleteDocumentIn)).FirstOrDefault();
        }

        //Delete a list of documents in the db
        public async Task<List<CouchDocumentChange>> DeleteDocuments<T>(List<T> Documents, string DatabaseToDeleteDocumentIn = null) where T : CouchDocument
        {
            //Get datbase to use
            DatabaseToDeleteDocumentIn = GetDBToUseForRequest(typeof(T), DatabaseToDeleteDocumentIn);
            //Create token if needed
            await Authenticate();
            //Ensure the document is marked as deleted
            Documents.ForEach(d => d.Deleted = true);
            //Use bulk operation to delete the list of documents
            var requestJSON = JSONHelper.ToJSON(new CouchBulk<T>(Documents));
            var requstURL = RequestHelper.BuildURL(DatabaseToDeleteDocumentIn, CouchEntryPoints.BulkOperation);
            var requestResult = await HttpPost(requstURL, requestJSON);
            var responseList = FromResponse<List<CouchDocumentChange>>(requestResult);
            //Update Revision numbers for caller
            responseList.ForEach(response => { if (response.Ok) Documents.First(d => d.ID == response.ID).Rev = response.Rev; });
            return responseList;
        }

        /// <summary>
        /// Update a document in CouchDb
        /// </summary>
        /// <typeparam name="T">Type of the document</typeparam>
        /// <param name="Document">Document to update</param>
        /// <param name="DatabaseToUpdateDocumentIn">Optional, set the db name</param>
        /// <returns>The change record from CouchDb</returns>
        public async Task<CouchDocumentChange> UpdateDocument<T>(T Document, string DatabaseToUpdateDocumentIn = null) where T : CouchDocument
        {
            //Call bulk update db will be set in there
            return (await UpdateDocuments<T>(new List<T>() { Document }, DatabaseToUpdateDocumentIn)).FirstOrDefault();
        }

        /// <summary>
        /// Bulk Update,Create,Delete Documents
        /// </summary>
        /// <typeparam name="T">Document type to change</typeparam>
        /// <param name="Documents">List of documents</param>
        /// <param name="DatabaseToUpdateDocumentsIn">Optional, set the db name</param>
        /// <returns>Change set for all documents</returns>
        public async Task<List<CouchDocumentChange>> UpdateDocuments<T>(List<T> Documents, string DatabaseToUpdateDocumentsIn = null) where T : CouchDocument
        {
            DatabaseToUpdateDocumentsIn = GetDBToUseForRequest(typeof(T), DatabaseToUpdateDocumentsIn);
            //Ensure IDS are created if setting is active
            if (ClientSettings.AutoGenerateID)
            {
                var documentswithMissingIDs = Documents.Where(d => string.IsNullOrEmpty(d.ID)).ToList();
                if (documentswithMissingIDs.Any())
                {
                    List<string> newGeneratedIds = null;
                    //Swith between CouchID and GUID 
                    if (ClientSettings.UseCouchUUID)
                        newGeneratedIds = (await GetManyUUIDs(documentswithMissingIDs.Count)).UUIDS;
                    else
                        newGeneratedIds = CreateRequestedAmountOfUUID(documentswithMissingIDs.Count);
                    for (var i = 0; i < documentswithMissingIDs.Count; ++i)
                    {
                        documentswithMissingIDs[i].ID = newGeneratedIds[i];
                    }
                }

            }
            //Use bulk to update list of documents in given db
            var requestURL = RequestHelper.BuildURL(DatabaseToUpdateDocumentsIn, CouchEntryPoints.BulkOperation);
            var requestJson = JSONHelper.ToJSON(new CouchBulk<T>(Documents));
            var requestResult = await HttpPost(requestURL, requestJson);
            var updateCallResult = FromResponse<List<CouchDocumentChange>>(requestResult);
            //Set new Revisions
            updateCallResult.Where(couchDocumentChange => couchDocumentChange.Ok)
                            .ToList()
                            .ForEach(couchDocumentChange =>
                {
                    var current_doc = Documents.FirstOrDefault(d => d.ID == couchDocumentChange.ID);
                    if (current_doc != null)
                        current_doc.Rev = couchDocumentChange.Rev;
                }
            );
            return updateCallResult;
        }

        /// <summary>
        /// Basic function to get a list of documents, allows the usage of views or any multi response URL
        /// </summary>
        /// <typeparam name="T">Type of documents</typeparam>
        /// <param name="RequestURL">URL for GET request</param>
        /// <returns>Generic document response for T</returns>
        public async Task<CouchDocumentResponse<T>> GetDocuments<T>(string RequestURL) where T : new()
        {
            //Create token if needed
            await Authenticate();
            //Get request for given URL, no db magic
            var result = await RequestHelper.Get(RequestURL);
            return FromResponse<CouchDocumentResponse<T>>(result);
        }

        /// <summary>
        /// Get a single document, optional with Rev number
        /// </summary>
        /// <typeparam name="T">Document Type</typeparam>
        /// <param name="ID">ID of the document to get</param>
        /// <param name="Revision">Optional, revision of the document</param>
        /// <param name="DatabaseToUse">Optional, set the db name</param>
        /// <returns>Document from Db</returns>
        public async Task<T> GetDocument<T>(string ID, string Revision = null, string DatabaseToUse = null) where T : new()
        {
            //Document == null -> check if entity has meta 
            DatabaseToUse = GetDBToUseForRequest(typeof(T), DatabaseToUse);
            //Create token if needed
            await Authenticate();
            var result = await RequestHelper.GetDocument(ID, DatabaseToUse, Revision: Revision);
            return FromResponse<T>(result);
        }

        /// <summary>
        /// Get all documents in the given database
        /// </summary>
        /// <param name="Document">Optional, Document type, used to get the DB</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDocumentResponse<CouchViewResponse<AllDocResponse>>> GetAllDocuments(Type Document = null, string DatabaseToUse = null)
        {
            DatabaseToUse = GetDBToUseForRequest(Document, DatabaseToUse); // Get database to use
            //TODO Ugly response to many <> and remove URL building in client
            return JSONHelper.FromJSON<CouchDocumentResponse<CouchViewResponse<AllDocResponse>>>(await RequestHelper.Get(RequestHelper.BuildURL(DatabaseToUse, CouchEntryPoints.AllDocs))); //return result to client
        }

        /// <summary>
        /// Add an attachment to an existing document
        /// </summary>
        /// <typeparam name="T">Type of document</typeparam>
        /// <param name="Document">The document to attach the file</param>
        /// <param name="AttachmentName">Name of the attachment</param>
        /// <param name="PathToFile">Path to the file</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDocumentChange> AddAttachment<T>(T Document, string AttachmentName, string PathToFile, string DatabaseToUse = null) where T : CouchDocument
        {
            if (!System.IO.File.Exists(PathToFile))
                throw new PillowException($"Unable to find file {PathToFile}!");
            DatabaseToUse = GetDBToUseForRequest(typeof(T), DatabaseToUse);
            var result = JSONHelper.FromJSON<CouchDocumentChange>(await RequestHelper.UploadFile(Document.ID, AttachmentName, Document.Rev, DatabaseToUse, PathToFile));
            if (result.Ok)
            {
                Document.Rev = result.Rev; // Update Revision number for caller
            }
            return result;
        }

        /// <summary>
        /// Download an attachment from a document
        /// </summary>
        /// <typeparam name="T">Document type to use</typeparam>
        /// <param name="Document">Document with attachment</param>
        /// <param name="AttachmentName">Name of the attachment to download</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<byte[]> GetAttachement<T>(T Document, string AttachmentName, string DatabaseToUse = null) where T : CouchDocument
        {
            DatabaseToUse = GetDBToUseForRequest(typeof(T), DatabaseToUse);
            await Authenticate();
            //Ask for file data
            var response = await RequestHelper.GetFile(Document.ID, AttachmentName, Document.Rev, DatabaseToUse);
            //In case something went wrong throw an error
            if (response.ResponseCode != HttpStatusCode.OK && response.ResponseCode != HttpStatusCode.Created)
                PillowErrorHelper.HandleNoneOKResponse(response, JSONHelper);
            //Return file data
            return response.RawBytes;
        }

        /// <summary>
        /// Delete an attachment from a document
        /// </summary>
        /// <typeparam name="T">Document type to use</typeparam>
        /// <param name="Document">The document with attachment</param>
        /// <param name="AttributeName">Name of the attachment</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDocumentChange> DeleteAttachment<T>(T Document, string AttributeName, string DatabaseToUse = null) where T : CouchDocument
        {
            DatabaseToUse = GetDBToUseForRequest(typeof(T), DatabaseToUse);
            await Authenticate();
            var result = JSONHelper.FromJSON<CouchDocumentChange>(await RequestHelper.DeleteFile(Document.ID, AttributeName, Document.Rev, DatabaseToUse));
            if (result.Ok)
            {
                Document.Rev = result.Rev;
            }
            return result;
        }
        //Ensure the ID is valid for a desgine document 
        private string EnsureDocumentIDIsValidDesignDocumentID(string DocumentID)
        {
            if (string.IsNullOrEmpty(DocumentID))
                throw new PillowException("Design document ID can´t be null");
            if (!DocumentID.StartsWith(CouchEntryPoints.DesignDoc))
                DocumentID = RequestHelper.BuildURL(CouchEntryPoints.DesignDoc, DocumentID);
            return DocumentID;
        }

        /// <summary>
        /// Get a view from CouchDB
        /// </summary>
        /// <typeparam name="T">Document type for view</typeparam>
        /// <param name="DocumentID">Id of the design document with the view</param>
        /// <param name="ViewName">Name of the view insiide the document</param>
        /// <param name="QueryParameter">Optional query parameter for the view</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDocumentResponse<T>> GetView<T>(string DocumentID, string ViewName, KeyValuePair<string, object>[] QueryParameter = null, string DatabaseToUse = null) where T : new()
        {
            DatabaseToUse = GetDBToUseForRequest(typeof(T), DatabaseToUse);
            await Authenticate();
            DocumentID = EnsureDocumentIDIsValidDesignDocumentID(DocumentID);
            return JSONHelper.FromJSON<CouchDocumentResponse<T>>(await RequestHelper.View(DatabaseToUse, DocumentID, ViewName, QueryParameter, HttpRequestMethod.GET, null));
        }

        /// <summary>
        /// Run a filter view 
        /// </summary>
        /// <typeparam name="T">Document type for view</typeparam>
        /// <param name="DocumentID">Id of the design document with the view</param>
        /// <param name="ViewName">Name of the view insiide the document</param>
        /// <param name="ViewFilter">List of keys to be used as a filter</param>
        /// <param name="QueryParameter">Optional query parameter for the view</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDocumentResponse<T>> FilterView<T>(string DocumentID, string ViewName, CouchViewFilter ViewFilter, KeyValuePair<string, object>[] QueryParameter = null, string DatabaseToUse = null) where T : new()
        {
            DatabaseToUse = GetDBToUseForRequest(typeof(T), DatabaseToUse);
            await Authenticate();
            DocumentID = EnsureDocumentIDIsValidDesignDocumentID(DocumentID);
            return JSONHelper.FromJSON<CouchDocumentResponse<T>>(await RequestHelper.View(DatabaseToUse, DocumentID, ViewName, QueryParameter, HttpRequestMethod.POST, JSONHelper.ToJSON(ViewFilter)));
        }

        /// <summary>
        /// Get a design document
        /// </summary>
        /// <param name="DocumentID">ID of the desgin document</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDesignDocument> GetDesignDocument(string DocumentID, string DatabaseToUse = null)
        {
            DatabaseToUse = GetDBToUseForRequest(null, DatabaseToUse);
            await Authenticate();
            DocumentID = EnsureDocumentIDIsValidDesignDocumentID(DocumentID);
            return JSONHelper.FromJSON<CouchDesignDocument>(await RequestHelper.Get(RequestHelper.BuildURL(DatabaseToUse, DocumentID)));
        }

        /// <summary>
        /// Create or update a desgin document
        /// </summary>
        /// <param name="DesignDocument">The desgin document to use</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<CouchDocumentChange> UpsertDesignDocument(CouchDesignDocument DesignDocument, string DatabaseToUse = null)
        {
            DatabaseToUse = GetDBToUseForRequest(DesignDocument.GetType(), DatabaseToUse);
            await Authenticate();
            if (string.IsNullOrEmpty(DesignDocument.ID) && ClientSettings.AutoGenerateID)
            {
                if (ClientSettings.UseCouchUUID)
                    DesignDocument.ID = await GetSingleUUID();
                else
                    DesignDocument.ID = Guid.NewGuid().ToString();
            }
            DesignDocument.ID = EnsureDocumentIDIsValidDesignDocumentID(DesignDocument.ID);
            var result = JSONHelper.FromJSON<CouchDocumentChange>(await RequestHelper.Put(RequestHelper.BuildURL(DatabaseToUse, DesignDocument.ID), JSONHelper.ToJSON(DesignDocument)));
            if (result.Ok)
            {
                DesignDocument.ID = result.ID;
                DesignDocument.Rev = result.Rev;
            }
            return result;
        }
        /// <summary>
        /// Gets you the current Revision of the document in CouchDB
        /// </summary>
        /// <param name="DocumentID">Document to check</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public async Task<string> GetCurrentDocumentRevision(string DocumentID,Type CouchDocumentType=null,string DatabaseToUse = null)
        {
            DatabaseToUse = GetDBToUseForRequest(CouchDocumentType, DatabaseToUse);
            var result = await RequestHelper.Head(RequestHelper.BuildURL(DatabaseToUse, DocumentID));
            return result.Header.Where(h => h.Key == "ETag").Select(h => h.Value).FirstOrDefault()?.Trim('"');
        }
        #endregion

        #region Private Functions        
        //Get the DB to use
        //Priority list:
        // 1. Caller Parameter
        // 2. Class DB, see Database Property
        // 3. DBNameAttribute of the current document
        private string GetDBToUseForRequest(Type Document, string Database)
        {
            //If DB provided by caller nothing to do
            if (string.IsNullOrEmpty(Database))
            {

                if (Document != null)
                {
                    //Last try check if document type has DBNameAttribute set
                    Database = (Document.GetTypeInfo().GetCustomAttribute(typeof(DBNameAttribute)) as DBNameAttribute)?.Name;
                }
                //Override with class DB if set
                if (string.IsNullOrEmpty(Database))
                    Database = this.ForcedDatabaseName;

                //Fail if no DB was found
                if (string.IsNullOrEmpty(Database))
                    throw new PillowException("No database set, unable to request");
            }
            //Return the db that was found
            return Database;
        }

        //Shorthand fucntion for simple post request
        private Task<RestResponse> HttpPost(string URL, string Body)
        {
            return RequestHelper.Post(URL, Body);
        }

        //Internal, convert all response to the given object
        private T FromResponse<T>(RestResponse Response) where T : new()
        {
            return JSONHelper.FromJSON<T>(Response);
        }

        //Create a list of GUIDS as string 
        private List<String> CreateRequestedAmountOfUUID(int Amount = 1)
        {
            var i = 0;
            var result = new List<string>();
            while (i < Amount)
            {
                result.Add(Guid.NewGuid().ToString());
                i += 1;
            }
            return result;
        }
        #endregion



    }
}