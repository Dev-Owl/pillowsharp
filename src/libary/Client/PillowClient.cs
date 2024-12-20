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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PillowSharp.Client
{
    public class PillowClient
    {
        /// <summary>
        /// Settings for this client
        /// </summary>
        public PillowClientSettings ClientSettings { get; set; } = new PillowClientSettings();

        ///<summary>
        ///Client version like 2.0.0
        ///</summary>
        public string Version { get; } = "4.0.3";

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

        private bool trace = false;
        public bool Trace
        {
            get
            {
                return trace;
            }
            set
            {
                trace = value;
                if (RequestHelper is BasicWebRequestHelper basicWebRequest)
                {
                    basicWebRequest.Trace = trace;
                }
            }
        }

        private Action<TraceInformation> traceCallback;
        public Action<TraceInformation> TraceCallback
        {
            get
            {
                return traceCallback;
            }
            set
            {
                traceCallback = value;
                if (RequestHelper is BasicWebRequestHelper basicWebRequest)
                {
                    basicWebRequest.TraceCallBack = value;
                }
            }
        }

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



        #region Server/DB Functions

        /// <summary>
        /// If Token authentication is used, login and store the token or add an existing to the current request
        /// </summary>
        /// <returns></returns>
        public Task AuthenticateAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                this.Authenticate();
            });
        }

        /// <summary>
        /// If Token authentication is used, login and store the token or add an existing to the current request
        /// </summary>
        /// <returns></returns>
        public void Authenticate()
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
                    var restSessionCreateResponse = RequestHelper.Post(CouchEntryPoints.Session, JSONHelper.ToJSON(currentLoginData));
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
        public Task<string> GetSingleUUIDAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetSingleUUID();
            });
        }

        /// <summary>
        /// Get IDs ,UUID, from couchDB by default one 
        /// </summary>
        /// <returns>New UUID</returns>
        public string GetSingleUUID()
        {
            var result = GetUUIDs(AmountOfUUIDs: 1);
            return result.UUIDS.FirstOrDefault();
        }

        /// <summary>
        /// Get IDs ,UUID, from couchDB
        /// </summary>
        /// <param name="AmountOfUUIDs">Number of ID�s you need</param>
        /// <returns>List of ID�s</returns>
        public Task<CouchUUIDResponse> GetManyUUIDsAsync(int AmountOfUUIDs)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetManyUUIDs(AmountOfUUIDs);
            });
        }

        /// <summary>
        /// Get IDs ,UUID, from couchDB
        /// </summary>
        /// <param name="AmountOfUUIDs">Number of ID�s you need</param>
        /// <returns>List of IDs</returns>
        public CouchUUIDResponse GetManyUUIDs(int AmountOfUUIDs)
        {
            if (AmountOfUUIDs <= 0)
            {
                throw new ArgumentOutOfRangeException("You must request 1 or more UUIDS", "AmountOfUUIDs");
            }
            return GetUUIDs(AmountOfUUIDs);
        }

        private CouchUUIDResponse GetUUIDs(int AmountOfUUIDs = 1)
        {
            //Create token if needed
            Authenticate();
            //Make the request and return the list of IDS 
            return JSONHelper.FromJSON<CouchUUIDResponse>(RequestHelper.Get(CouchEntryPoints.NewUUID, new KeyValuePair<string, object>(CouchEntryPoints.ParamCount, AmountOfUUIDs)));
        }

        /// <summary>
        /// Check if a db exists in the current server
        /// </summary>
        /// <param name="Name">Database name</param>
        /// <returns></returns>
        public Task<bool> DbExistsAsync(string Name)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DbExists(Name);
            });
        }

        /// <summary>
        /// Check if a db exists in the current server
        /// </summary>
        /// <param name="Name">Database name</param>
        /// <returns></returns>
        public bool DbExists(string Name)
        {
            //Create token if needed
            Authenticate();
            //Get list of all db and check if included
            return GetListOfAllDatabases().Contains(Name);
        }

        /// <summary>
        /// Gets the list of replication jobs from the scheduler
        /// </summary>
        /// <returns>SchedulerResponse containing active replication jobs</returns>
        public Task<SchedulerResponse> GetSchedulerJobsAsync(int? limit = null, int? skip = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetSchedulerJobs(limit, skip);
            });
        }

        /// <summary>
        /// Gets the list of replication jobs from the scheduler
        /// </summary>
        /// <returns>SchedulerResponse containing active replication jobs</returns>
        public SchedulerResponse GetSchedulerJobs(int? limit = null, int? skip = null)
        {
            //Create token if needed
            Authenticate();
            //Make the request to get the scheduler jobs
            var result = RequestHelper.Get(CouchEntryPoints.SchedulerJobs);
            //return result
            return FromResponse<SchedulerResponse>(result);
        }

        /// <summary>
        /// Triggers a replication operation between source and target databases
        /// </summary>
        /// <param name="request">The replication configuration</param>
        /// <returns>Response containing replication details</returns>
        public Task<ReplicationResponse> CreateReplicationAsync(ReplicationRequest request)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.CreateReplication(request);
            });
        }

        /// <summary>
        /// Triggers a replication operation between source and target databases
        /// </summary>
        /// <param name="request">The replication configuration</param>
        /// <returns>Response containing replication details</returns>
        public ReplicationResponse CreateReplication(ReplicationRequest request)
        {
            //Create token if needed
            Authenticate();
            //Make the request to start replication
            var result = RequestHelper.Post(CouchEntryPoints.Replicate, JSONHelper.ToJSON(request));
            //return result
            return FromResponse<ReplicationResponse>(result);
        }




        /// <summary>
        /// Get a list of all current dbs
        /// </summary>
        /// <returns>List of db names</returns>
        public Task<List<string>> GetListOfAllDatabasesAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetListOfAllDatabases();
            });
        }

        /// <summary>
        /// Get a list of all current dbs
        /// </summary>
        /// <returns>List of db names</returns>
        public List<string> GetListOfAllDatabases()
        {
            //Create token if needed
            Authenticate();
            //Make the request to get the list of db
            var result = RequestHelper.Get(CouchEntryPoints.AllDBQuery);
            //return result
            return FromResponse<List<string>>(result);
        }

        /// <summary>
        /// Create a database on the server with the given name
        /// </summary>
        /// <param name="Name">New database name</param>
        /// <returns></returns>
        public Task<bool> CreateNewDatabaseAsync(string Name, bool Partitioned = false)
        {
            return Task.Factory.StartNew(() =>
            {
                return CreateNewDatabase(Name, Partitioned);
            });
        }

        /// <summary>
        /// Create a database on the server with the given name
        /// </summary>
        /// <param name="Name">New database name</param>
        /// <returns></returns>
        public bool CreateNewDatabase(string Name, bool Partitioned = false)
        {
            //Create token if needed
            Authenticate();
            KeyValuePair<string, object>[] queryParameter = null;
            if (Partitioned)
            {
                queryParameter = new[]{
                    new KeyValuePair<string, object>("partitioned", "true")
                };
            }
            //Create the db via a put call
            var result = RequestHelper.Put(Name, QueryParameter: queryParameter);
            //Return response state
            return FromResponse<CouchConfirm>(result)?.Ok ?? false;
        }

        public Task<CouchDbInfo> GetDatabaseInfoAsync(string Name)
        {
            return Task.Factory.StartNew(() =>
          {
              return GetDatabaseInfo(Name);
          });
        }

        public CouchDbInfo GetDatabaseInfo(string Name)
        {
            Authenticate();
            var result = RequestHelper.Get(Name);
            return FromResponse<CouchDbInfo>(result);
        }

        /// <summary>
        /// Delete a database on the server
        /// </summary>
        /// <param name="Name">Name of the database</param>
        /// <returns></returns>
        public Task<bool> DeleteDatabaseAsync(string Name)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.DeleteDatabase(Name);
            });
        }

        /// <summary>
        /// Delete a database on the server
        /// </summary>
        /// <param name="Name">Name of the database</param>
        /// <returns></returns>
        public bool DeleteDatabase(string Name)
        {
            Authenticate();
            return JSONHelper.FromJSON<CouchConfirm>(RequestHelper.Delete(Name))?.Ok ?? false;
        }

        /// <summary>
        /// Runs the provided query against a database
        /// </summary>
        /// <param name="query">Mango query to execute</param>
        /// <param name="database">Override any other db set for this client and run query on this db</param>
        /// <typeparam name="T">Type that is requested by the query</typeparam>
        /// <returns>Result of the query</returns>
        public Task<MangoQueryResult<T>> RunMangoQueryAsync<T>(MangoQuery query, string database = null, string partition = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.RunMangoQuery<T>(query, database, partition);
            });
        }

        /// <summary>
        /// Runs the provided query against a database
        /// </summary>
        /// <param name="query">Mango query to execute</param>
        /// <param name="database">Override any other db set for this client and run query on this db</param>
        /// <typeparam name="T">Type that is requested by the query</typeparam>
        /// <returns>Result of the query</returns>
        public MangoQueryResult<T> RunMangoQuery<T>(MangoQuery query, string database = null, string partition = null)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            //Ensure the query is at least following the rules, still open to fail later...
            query.Validate();
            var databaseForQuery = GetDatabaseAndAuthenticateIfNeeded(null, database);
            var urlParts = new List<string> { databaseForQuery, CouchEntryPoints.MangoQuery };
            if (!string.IsNullOrEmpty(partition))
            {
                urlParts.Insert(1, CouchEntryPoints.Partition);
                urlParts.Insert(2, partition);
            }
            var response = HttpPost(RequestHelper.BuildURL(urlParts.ToArray()), JSONHelper.ToJSON(query));
            return FromResponse<MangoQueryResult<T>>(response);
        }

        /// <summary>
        /// Create an index for Mango Queries
        /// </summary>
        /// <param name="newIndex">Details about the index</param>
        /// <param name="database">Database to use</param>
        /// <returns>State of the request</returns>
        public MangoIndexResponse CreateMangoIndex(MangoIndex newIndex, string database = null)
        {
            if (newIndex == null)
            {
                throw new ArgumentNullException(nameof(newIndex));
            }
            var databaseForQuery = GetDatabaseAndAuthenticateIfNeeded(null, database);
            var response = HttpPost(RequestHelper.BuildURL(databaseForQuery, CouchEntryPoints.MangoIndex), JSONHelper.ToJSON(newIndex));
            return FromResponse<MangoIndexResponse>(response);
        }

        /// <summary>
        /// Create an index for Mango Queries
        /// </summary>
        /// <param name="newIndex">Details about the index</param>
        /// <param name="database">Database to use</param>
        /// <returns>State of the request</returns>
        public Task<MangoIndexResponse> CreateMangoIndexAsync(MangoIndex newIndex, string database = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return CreateMangoIndex(newIndex, database);
            });
        }

        public PurgeResponse PurgeDocumentsInDatabase(PurgeRequest purgeRequest, string database = null)
        {
            if (purgeRequest is null)
            {
                throw new ArgumentNullException(nameof(purgeRequest));
            }
            var databaseForQuery = GetDatabaseAndAuthenticateIfNeeded(null, database);
            var response = HttpPost(RequestHelper.BuildURL(databaseForQuery, CouchEntryPoints.Purge), purgeRequest.WriteJosn());
            return FromResponse<PurgeResponse>(response);

        }

        public Task<PurgeResponse> PurgeDocumentsInDatabaseAsync(PurgeRequest purgeRequest, string database = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return PurgeDocumentsInDatabase(purgeRequest, database);
            });
        }


        public Task<PartitionInfo> GetPartitionInfoAsync(string PartitionId, string Database = null)
        {
            return Task.Factory.StartNew(() =>
                       {
                           return GetPartitionInfo(PartitionId, Database);
                       });
        }

        public PartitionInfo GetPartitionInfo(string PartitionId, string Database = null)
        {
            var databaseToUse = GetDatabaseAndAuthenticateIfNeeded(null, Database);
            var url = RequestHelper.BuildURL(databaseToUse, CouchEntryPoints.Partition, PartitionId);
            var response = RequestHelper.Get(url);
            return FromResponse<PartitionInfo>(response);
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
        public Task<CouchDocumentChange> CreateANewDocumentAsync<T>(T NewDocument, string DatabaseToCreateDocumentIn = null) where T : new()
        {
            return Task.Factory.StartNew(() =>
            {
                return CreateANewDocument<T>(NewDocument, DatabaseToCreateDocumentIn);
            });
        }

        /// <summary>
        /// Create a document in the given server and database
        /// </summary>
        /// <typeparam name="T">Type of the document</typeparam>
        /// <param name="NewDocument">The new document</param>
        /// <param name="DatabaseToCreateDocumentIn">Optional, set the db name</param>
        /// <returns>The change record from CouchDb</returns>
        public CouchDocumentChange CreateANewDocument<T>(T NewDocument, string DatabaseToCreateDocumentIn = null) where T : new()
        {
            //Get datbase to use
            DatabaseToCreateDocumentIn = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToCreateDocumentIn);

            if (NewDocument is CouchDocument)
            {
                var couchDoc = NewDocument as CouchDocument;
                if (string.IsNullOrEmpty(couchDoc.ID))
                {
                    if (ClientSettings.AutoGenerateID)
                        if (ClientSettings.UseCouchUUID)
                            couchDoc.ID = GetSingleUUID();
                        else
                            couchDoc.ID = Guid.NewGuid().ToString();
                }
            }
            //Request and build json for T
            var result = FromResponse<CouchDocumentChange>(HttpPost(DatabaseToCreateDocumentIn, JSONHelper.ToJSON(NewDocument)));
            //If result is ok and the new created entity is based on CouchDocument set values as returned by the server
            if (result.Ok && NewDocument is CouchDocument)
            {
                var couchDoc = NewDocument as CouchDocument;
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
        public Task<CouchDocumentChange> DeleteDocumentAsync<T>(T Document, string DatabaseToDeleteDocumentIn = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return DeleteDocument<T>(Document, DatabaseToDeleteDocumentIn);
            });
        }

        /// <summary>
        /// Delete a single document in the db
        /// </summary>
        /// <typeparam name="T">Type of the document</typeparam>
        /// <param name="Document">Document to delete</param>
        /// <param name="DatabaseToDeleteDocumentIn">Optional, set the db name</param>
        /// <returns>The change record from CouchDb</returns>
        public CouchDocumentChange DeleteDocument<T>(T Document, string DatabaseToDeleteDocumentIn = null) where T : CouchDocument
        {
            return DeleteDocuments<T>(new List<T>() { Document }, DatabaseToDeleteDocumentIn).FirstOrDefault();
        }

        //Delete a list of documents in the db
        public Task<List<CouchDocumentChange>> DeleteDocumentsAsync<T>(List<T> Documents, string DatabaseToDeleteDocumentIn = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return DeleteDocuments<T>(Documents, DatabaseToDeleteDocumentIn);
            });
        }
        //Delete a list of documents in the db
        public List<CouchDocumentChange> DeleteDocuments<T>(List<T> Documents, string DatabaseToDeleteDocumentIn = null) where T : CouchDocument
        {
            //Get datbase to use
            DatabaseToDeleteDocumentIn = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToDeleteDocumentIn);
            //Ensure the document is marked as deleted
            Documents.ForEach(d => d.Deleted = true);
            //Use bulk operation to delete the list of documents
            var requestJSON = JSONHelper.ToJSON(new CouchBulk<T>(Documents));
            var requstURL = RequestHelper.BuildURL(DatabaseToDeleteDocumentIn, CouchEntryPoints.BulkOperation);
            var requestResult = HttpPost(requstURL, requestJSON);
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
        public Task<CouchDocumentChange> UpdateDocumentAsync<T>(T Document, string DatabaseToUpdateDocumentIn = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return UpdateDocument<T>(Document, DatabaseToUpdateDocumentIn);
            });
        }

        /// <summary>
        /// Update a document in CouchDb
        /// </summary>
        /// <typeparam name="T">Type of the document</typeparam>
        /// <param name="Document">Document to update</param>
        /// <param name="DatabaseToUpdateDocumentIn">Optional, set the db name</param>
        /// <returns>The change record from CouchDb</returns>
        public CouchDocumentChange UpdateDocument<T>(T Document, string DatabaseToUpdateDocumentIn = null) where T : CouchDocument
        {
            //Call bulk update db will be set in there
            return UpdateDocuments<T>(new List<T>() { Document }, DatabaseToUpdateDocumentIn).FirstOrDefault();
        }
        /// <summary>
        /// Bulk Update,Create,Delete Documents
        /// </summary>
        /// <typeparam name="T">Document type to change</typeparam>
        /// <param name="Documents">List of documents</param>
        /// <param name="DatabaseToUpdateDocumentsIn">Optional, set the db name</param>
        /// <returns>Change set for all documents</returns>
        public Task<List<CouchDocumentChange>> UpdateDocumentsAsync<T>(List<T> Documents, string DatabaseToUpdateDocumentsIn = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return UpdateDocuments<T>(Documents, DatabaseToUpdateDocumentsIn);
            });
        }
        /// <summary>
        /// Bulk Update,Create,Delete Documents
        /// </summary>
        /// <typeparam name="T">Document type to change</typeparam>
        /// <param name="Documents">List of documents</param>
        /// <param name="DatabaseToUpdateDocumentsIn">Optional, set the db name</param>
        /// <returns>Change set for all documents</returns>
        public List<CouchDocumentChange> UpdateDocuments<T>(List<T> Documents, string DatabaseToUpdateDocumentsIn = null) where T : CouchDocument
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
                        newGeneratedIds = GetManyUUIDs(documentswithMissingIDs.Count).UUIDS;
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
            var requestResult = HttpPost(requestURL, requestJson);
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
        public Task<CouchDocumentResponse<T>> GetDocumentsAsync<T>(string RequestURL) where T : new()
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetDocuments<T>(RequestURL);
            });
        }

        /// <summary>
        /// Basic function to get a list of documents, allows the usage of views or any multi response URL
        /// </summary>
        /// <typeparam name="T">Type of documents</typeparam>
        /// <param name="RequestURL">URL for GET request</param>
        /// <returns>Generic document response for T</returns>
        public CouchDocumentResponse<T> GetDocuments<T>(string RequestURL) where T : new()
        {
            //Create token if needed
            Authenticate();
            //Get request for given URL, no db magic
            var result = RequestHelper.Get(RequestURL);
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
        public Task<T> GetDocumentAsync<T>(string ID, string Revision = null, string DatabaseToUse = null) where T : new()
        {
            return Task.Factory.StartNew(() =>
            {
                return GetDocument<T>(ID, Revision, DatabaseToUse);
            });

        }

        /// <summary>
        /// Get a single document, optional with Rev number
        /// </summary>
        /// <typeparam name="T">Document Type</typeparam>
        /// <param name="ID">ID of the document to get</param>
        /// <param name="Revision">Optional, revision of the document</param>
        /// <param name="DatabaseToUse">Optional, set the db name</param>
        /// <returns>Document from Db</returns>
        public T GetDocument<T>(string ID, string Revision = null, string DatabaseToUse = null) where T : new()
        {
            //Document == null -> check if entity has meta 
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            var result = RequestHelper.GetDocument(ID, DatabaseToUse, Revision: Revision);
            return FromResponse<T>(result);
        }
        /// <summary>
        /// Get all documents in the given database
        /// </summary>
        /// <param name="Document">Optional, Document type, used to get the DB</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public Task<CouchDocumentResponse<CouchViewResponse<AllDocResponse>>> GetAllDocumentsAsync(Type Document = null, string DatabaseToUse = null, string PartitionToUse = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetAllDocuments(Document, DatabaseToUse, PartitionToUse);
            });
        }

        /// <summary>
        /// Get all documents in the given database
        /// </summary>
        /// <param name="Document">Optional, Document type, used to get the DB</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public CouchDocumentResponse<CouchViewResponse<AllDocResponse>> GetAllDocuments(Type Document = null, string DatabaseToUse = null, string PartitionToUse = null)
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(Document, DatabaseToUse); // Get database to use
            var urlParts = new List<string> { DatabaseToUse };
            if (string.IsNullOrEmpty(PartitionToUse) == false)
            {
                urlParts.Add(CouchEntryPoints.Partition);
                urlParts.Add(PartitionToUse);
            }
            urlParts.Add(CouchEntryPoints.AllDocs);

            return JSONHelper.FromJSON<CouchDocumentResponse<CouchViewResponse<AllDocResponse>>>(
                RequestHelper.Get(RequestHelper.BuildURL(urlParts.ToArray()))); //return result to client
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
        public Task<CouchDocumentChange> AddAttachmentAsync<T>(T Document, string AttachmentName, string PathToFile, string DatabaseToUse = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return this.AddAttachment<T>(Document, AttachmentName, PathToFile, DatabaseToUse);
            });
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
        public CouchDocumentChange AddAttachment<T>(T Document, string AttachmentName, string PathToFile, string DatabaseToUse = null) where T : CouchDocument
        {
            if (!System.IO.File.Exists(PathToFile))
                throw new PillowException($"Unable to find file {PathToFile}!");
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            var result = JSONHelper.FromJSON<CouchDocumentChange>(
                RequestHelper.UploadFile(Document.ID, AttachmentName,
                                         Document.Rev, DatabaseToUse, PathToFile));
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
        public Task<byte[]> GetAttachementAsync<T>(T Document, string AttachmentName, string DatabaseToUse = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return GetAttachement<T>(Document, AttachmentName, DatabaseToUse);
            });
        }

        /// <summary>
        /// Download an attachment from a document
        /// </summary>
        /// <typeparam name="T">Document type to use</typeparam>
        /// <param name="Document">Document with attachment</param>
        /// <param name="AttachmentName">Name of the attachment to download</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public byte[] GetAttachement<T>(T Document, string AttachmentName, string DatabaseToUse = null) where T : CouchDocument
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            //Ask for file data
            var response = RequestHelper.GetFile(Document.ID, AttachmentName, Document.Rev, DatabaseToUse);
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
        public Task<CouchDocumentChange> DeleteAttachmentAsync<T>(T Document, string AttributeName, string DatabaseToUse = null) where T : CouchDocument
        {
            return Task.Factory.StartNew(() =>
            {
                return DeleteAttachment<T>(Document, AttributeName, DatabaseToUse);
            });
        }

        /// <summary>
        /// Delete an attachment from a document
        /// </summary>
        /// <typeparam name="T">Document type to use</typeparam>
        /// <param name="Document">The document with attachment</param>
        /// <param name="AttributeName">Name of the attachment</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public CouchDocumentChange DeleteAttachment<T>(T Document, string AttributeName, string DatabaseToUse = null) where T : CouchDocument
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            var result = JSONHelper.FromJSON<CouchDocumentChange>(
                                    RequestHelper.DeleteFile(Document.ID, AttributeName, Document.Rev, DatabaseToUse));
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
                throw new PillowException("Design document ID cant be null");
            if (!DocumentID.StartsWith(CouchEntryPoints.DesignDoc))
                DocumentID = RequestHelper.BuildURL(CouchEntryPoints.DesignDoc, DocumentID);
            return DocumentID;
        }

        /// <summary>
        /// Run the same action on Multiple Dbs
        /// </summary>
        /// <param name="action">Called for each database, provided the database name</param>
        /// <param name="databaseList">Name of the view insiide the document</param>
        /// <returns></returns>
        public void RunForMultipleDatabase(Action<string> action, List<string> databaseList)
        {
            databaseList.ForEach(db => action.Invoke(db));
        }

        /// <summary>
        /// Run the same action on Multiple Dbs asyn
        /// </summary>
        /// <param name="action">Called for each database, provided the database name</param>
        /// <param name="databaseList">Name of the view insiide the document</param>
        /// <returns></returns>
        public Task RunForMultipleDatabaseAsync(Action<string> action, List<string> databaseList)
        {
            return Task.Factory.StartNew(() =>
            {
                RunForMultipleDatabase(action, databaseList);
            });
        }

        /// <summary>
        /// Run the same action on all dbs with an optional filter
        /// </summary>
        /// <param name="action">Called for each database, provided the database name</param>
        /// <param name="dbFilter">Optional filter, provided the database if true will call action</param>
        /// <returns></returns>
        public void RunForAllDbs(Action<string> action, Func<string, bool> dbFilter = null)
        {
            var allDbs = GetListOfAllDatabases();
            if (dbFilter == null)
            {
                RunForMultipleDatabase(action, allDbs);
            }
            else
            {
                RunForMultipleDatabase(action, allDbs.Where(databse => dbFilter.Invoke(databse)).ToList());
            }
        }

        /// <summary>
        /// Run the same action on all dbs with an optional filter asyn
        /// </summary>
        /// <param name="action">Called for each database, provided the database name</param>
        /// <param name="dbFilter">Optional filter, provided the database if true will call action</param>
        /// <returns></returns>
        public Task RunForAllDbsAsyn(Action<string> action, Func<string, bool> dbFilter = null)
        {
            return Task.Factory.StartNew(() =>
            {
                RunForAllDbs(action, dbFilter);
            });
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
        public Task<CouchDocumentResponse<T>> GetViewAsync<T>(string DocumentID, string ViewName, KeyValuePair<string, object>[] QueryParameter = null, string DatabaseToUse = null, string partition = null) where T : new()
        {
            return Task.Factory.StartNew(() =>
            {
                return GetView<T>(DocumentID, ViewName, QueryParameter, DatabaseToUse, partition);
            });
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
        public CouchDocumentResponse<T> GetView<T>(string DocumentID, string ViewName, KeyValuePair<string, object>[] QueryParameter = null, string DatabaseToUse = null, string partition = null) where T : new()
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            DocumentID = EnsureDocumentIDIsValidDesignDocumentID(DocumentID);
            return JSONHelper.FromJSON<CouchDocumentResponse<T>>(
                RequestHelper.View(DatabaseToUse, DocumentID, ViewName, QueryParameter, HttpRequestMethod.GET, null, partition));
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
        public Task<CouchDocumentResponse<T>> FilterViewAsync<T>(string DocumentID, string ViewName, CouchViewFilter ViewFilter, KeyValuePair<string, object>[] QueryParameter = null, string DatabaseToUse = null) where T : new()
        {
            return Task.Factory.StartNew(() =>
            {
                return FilterView<T>(DocumentID, ViewName, ViewFilter, QueryParameter, DatabaseToUse);
            });
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
        public CouchDocumentResponse<T> FilterView<T>(string DocumentID, string ViewName, CouchViewFilter ViewFilter, KeyValuePair<string, object>[] QueryParameter = null, string DatabaseToUse = null) where T : new()
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            DocumentID = EnsureDocumentIDIsValidDesignDocumentID(DocumentID);
            return JSONHelper.FromJSON<CouchDocumentResponse<T>>(
                RequestHelper.View(DatabaseToUse, DocumentID, ViewName,
                                   QueryParameter, HttpRequestMethod.POST,
                                   JSONHelper.ToJSON(ViewFilter)));
        }

        public Task<T> GetListFromViewAsync<T>(string designDocumentID, string listName, string viewName, KeyValuePair<string, object>[] queryParameter, string databaseToUse = null) where T : new()
        {
            return Task.Factory.StartNew(() =>
            {
                return GetListFromView<T>(designDocumentID, listName, viewName, queryParameter, databaseToUse);
            });
        }

        public T GetListFromView<T>(string designDocumentID, string listName, string viewName, KeyValuePair<string, object>[] queryParameter, string databaseToUse = null) where T : new()
        {
            return JSONHelper.FromJSON<T>(RunListFunction(designDocumentID, listName, viewName, queryParameter, databaseToUse));
        }

        private RestResponse RunListFunction(string designDocumentID, string listName, string viewName, KeyValuePair<string, object>[] queryParameter, string databaseToUse = null)
        {
            databaseToUse = GetDatabaseAndAuthenticateIfNeeded(null, databaseToUse);
            designDocumentID = EnsureDocumentIDIsValidDesignDocumentID(designDocumentID);
            var response = RequestHelper.List(databaseToUse, designDocumentID, listName, viewName, queryParameter);
            if (response.ResponseCode == HttpStatusCode.OK || response.ResponseCode == HttpStatusCode.Created)
            {
                return response;
            }
            else
            {
                PillowErrorHelper.HandleNoneOKResponse(response, JSONHelper);
                return null;
            }
        }

        public Task<string> GetListFromViewAsync(string designDocumentID, string listName, string viewName, KeyValuePair<string, object>[] queryParameter, string databaseToUse = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetListFromView(designDocumentID, listName, viewName, queryParameter, databaseToUse);
            });
        }

        public string GetListFromView(string designDocumentID, string listName, string viewName, KeyValuePair<string, object>[] queryParameter, string databaseToUse = null)
        {
            return RunListFunction(designDocumentID, listName, viewName, queryParameter, databaseToUse).Content;
        }

        public Task<T> RunUpdateFunctionAsync<T>(string DesignDocumentID, string UpdateFunctionName, string DocumentID = "", string DatabaseToUse = null, KeyValuePair<string, object>[] QueryParameter = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return RunUpdateFunction<T>(DesignDocumentID, UpdateFunctionName, DocumentID, DatabaseToUse, QueryParameter);
            });
        }

        public T RunUpdateFunction<T>(string DesignDocumentID, string UpdateFunctionName, string DocumentID = "", string DatabaseToUse = null, KeyValuePair<string, object>[] QueryParameter = null)
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(typeof(T), DatabaseToUse);
            var response = RequestHelper.Put(RequestHelper.BuildURL(DatabaseToUse,
                                            EnsureDocumentIDIsValidDesignDocumentID(DesignDocumentID),
                                            CouchEntryPoints.UpdateFunction,
                                            UpdateFunctionName,
                                            DocumentID), QueryParameter: QueryParameter);

            return (T)Convert.ChangeType(response.Content, typeof(T));

        }
        /// <summary>
        /// Get a design document
        /// </summary>
        /// <param name="DocumentID">ID of the desgin document</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public Task<CouchDesignDocument> GetDesignDocumentAsync(string DocumentID, string DatabaseToUse = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetDesignDocument(DocumentID, DatabaseToUse);
            });
        }

        /// <summary>
        /// Get a design document
        /// </summary>
        /// <param name="DocumentID">ID of the desgin document</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public CouchDesignDocument GetDesignDocument(string DocumentID, string DatabaseToUse = null)
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(null, DatabaseToUse);
            DocumentID = EnsureDocumentIDIsValidDesignDocumentID(DocumentID);
            return JSONHelper.FromJSON<CouchDesignDocument>(
                RequestHelper.Get(RequestHelper.BuildURL(DatabaseToUse, DocumentID)));
        }

        /// <summary>
        /// Create or update a desgin document
        /// </summary>
        /// <param name="DesignDocument">The desgin document to use</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public Task<CouchDocumentChange> UpsertDesignDocumentAsync(CouchDesignDocument DesignDocument, string DatabaseToUse = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return UpsertDesignDocument(DesignDocument, DatabaseToUse);
            });
        }


        /// <summary>
        /// Create or update a desgin document
        /// </summary>
        /// <param name="DesignDocument">The desgin document to use</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public CouchDocumentChange UpsertDesignDocument(CouchDesignDocument DesignDocument, string DatabaseToUse = null)
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(DesignDocument.GetType(), DatabaseToUse);
            if (string.IsNullOrEmpty(DesignDocument.ID) && ClientSettings.AutoGenerateID)
            {
                if (ClientSettings.UseCouchUUID)
                    DesignDocument.ID = GetSingleUUID();
                else
                    DesignDocument.ID = Guid.NewGuid().ToString();
            }
            DesignDocument.ID = EnsureDocumentIDIsValidDesignDocumentID(DesignDocument.ID);
            var result = JSONHelper.FromJSON<CouchDocumentChange>(RequestHelper.Put(RequestHelper.BuildURL(DatabaseToUse, DesignDocument.ID), JSONHelper.ToJSON(DesignDocument)));
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
        public Task<string> GetCurrentDocumentRevisionAsync(string DocumentID, Type CouchDocumentType = null, string DatabaseToUse = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetCurrentDocumentRevision(DocumentID, CouchDocumentType, DatabaseToUse);
            });
        }

        /// <summary>
        /// Gets you the current Revision of the document in CouchDB
        /// </summary>
        /// <param name="DocumentID">Document to check</param>
        /// <param name="DatabaseToUse">Optional, Database for this request</param>
        /// <returns></returns>
        public string GetCurrentDocumentRevision(string DocumentID, Type CouchDocumentType = null, string DatabaseToUse = null)
        {
            DatabaseToUse = GetDatabaseAndAuthenticateIfNeeded(CouchDocumentType, DatabaseToUse);
            var result = RequestHelper.Head(RequestHelper.BuildURL(DatabaseToUse, DocumentID));
            return result.Header.Where(h => h.Key == "ETag").Select(h => h.Value).FirstOrDefault()?.Trim('"');
        }
        #endregion

        #region Private Functions        


        /// <summary>
        /// Used to combine Authenticate and GetDBForRequest to a single endpoint, reduce duplicated code in each call
        /// </summary>
        /// <param name="Document">Current document</param>
        /// <param name="Database">Forced database for request</param>
        /// <returns>Database to use for request</returns>
        private string GetDatabaseAndAuthenticateIfNeeded(Type Document, string Database)
        {
            Authenticate();
            return GetDBToUseForRequest(Document, Database);
        }

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
        private RestResponse HttpPost(string URL, string Body)
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