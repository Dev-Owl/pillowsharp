using System;
using System.Net;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using RestSharp;
using RestSharp.Authenticators;
using PillowSharp.Helper;
using System.Linq;
using PillowSharp.Client;
using System.Collections.Generic;
using System.IO;
using PillowSharp.CouchType;
using System.Web;
using pillowsharp.Middleware;
using pillowsharp.Middleware.Default;
using pillowsharp.Helper;
using System.Diagnostics;

namespace PillowSharp.Middleware.Default
{
    public class BasicWebRequestHelper : AWebRequestHelper
    {
        //RestSharp client
        RestClient client = null;
        //Cookie storage for autehntication, optional
        CookieContainer cookieContainer = null;
        //URI to server, used for Cookie
        Uri serverURI = null;

        //If set call time is traced
        public bool Trace { get; set; }

        public Action<TraceInformation> TraceCallBack { get; set; }

        //Create the Middleware and pass server data to it
        public BasicWebRequestHelper(ICouchdbServer Server) : base(Server)
        {
            UpdateServerData(Server);
        }

        public override void UpdateServerData(ICouchdbServer Server)
        {
            RestClientOptions clientOptions = null;
            //Generate needed data for selected auth type
            if (Server.LoginType == ELoginTypes.BasicAuth)
            {
                clientOptions = new RestClientOptions(Server.GetServerURL())
                {
                    Authenticator = new HttpBasicAuthenticator(Server.GetLoginData().UserName, Server.GetLoginData().Password)
                };
            }
            else if (Server.LoginType == ELoginTypes.TokenLogin)
            {
                cookieContainer = new CookieContainer();
                clientOptions = new RestClientOptions(Server.GetServerURL())
                {
                    CookieContainer = cookieContainer
                };
                serverURI = new Uri(Server.GetServerURL());
            }
            else
            {
                clientOptions = new RestClientOptions(Server.GetServerURL());
            }



            //Creteate the rest clinet
            client = new RestClient(clientOptions);
        }

        private RestRequest BuildRequestBase(string Resource, Method Method = Method.Get, int TimeOut = 30, KeyValuePair<string, object>[] QueryParameter = null)
        {
            var request = new RestRequest
            {
                Method = Method,
                Timeout = new TimeSpan(0, 0, TimeOut),
                Resource = Resource
            };
            if (QueryParameter != null)
            {
                foreach (var keyValue in QueryParameter)
                {
                    if (keyValue.Key != null && keyValue.Value != null)
                        request.AddQueryParameter(keyValue.Key, keyValue.Value.ToString());
                }
            }
            return request;
        }
        private RestRequest AddJSONBody(RestRequest request, string Body)
        {
            //add a json body to the request
            request.AddParameter("application/json", Body, ParameterType.RequestBody); //todo add type as constant
            return request;
        }

        //Base function for all requests
        private Task<pillowsharp.Middleware.RestResponse> RequestAsync(RestRequest Request)
        {

            return Task.Factory.StartNew(() => { return this.Request(Request); });
            // return client.GetResponseAsync(Request).Then(Response => new RestSharpResponse(Response) as pillowsharp.Middleware.RestResponse);
        }

        private pillowsharp.Middleware.RestResponse Request(RestRequest Request)
        {
            var stopwatch = new Stopwatch();
            if (Trace)
            {
                stopwatch.Start();
            }
            var result = new RestSharpResponse(client.Execute(Request)) as pillowsharp.Middleware.RestResponse;
            if (Trace)
            {
                stopwatch.Stop();
                TraceCallBack?.Invoke(new TraceInformation()
                {
                    RequestTimeInMs = stopwatch.ElapsedMilliseconds,
                    RequestUrl = Request.Resource,
                    RequestMethod = Request.Method.ToString("g")
                });
            }
            return result;
        }

        public override pillowsharp.Middleware.RestResponse GetDocument(string ID, string Database, string Revision = null)
        {
            return Get(BuildURL(Database, ID), new KeyValuePair<string, object>(Rev, Revision));
        }
        //Get a single document with the given revesion number from the db
        public override Task<pillowsharp.Middleware.RestResponse> GetDocumentAsync(string ID, string Database, string Revision = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.GetDocument(ID, Database, Revision);
            });
        }

        //Get request to the given URL
        public override pillowsharp.Middleware.RestResponse Get(string Url)
        {
            return Request(BuildRequestBase(Url));
        }

        public override Task<pillowsharp.Middleware.RestResponse> GetAsync(string Url, params KeyValuePair<string, object>[] Parameter)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.Get(Url, Parameter);
            });
        }
        //Get request to the given URL include Request parameters
        public override pillowsharp.Middleware.RestResponse Get(string Url, params KeyValuePair<string, object>[] Parameter)
        {
            var getRequest = BuildRequestBase(Url);
            if (Parameter != null)
            {
                foreach (var keyValue in Parameter)
                {
                    if (keyValue.Key != null && keyValue.Value != null)
                        getRequest.AddQueryParameter(keyValue.Key, keyValue.Value.ToString());
                }
            }
            return Request(getRequest);
        }

        public override Task<pillowsharp.Middleware.RestResponse> PutAsync(string Url, string Body = null, KeyValuePair<string, object>[] QueryParameter = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.Put(Url, Body, QueryParameter);
            });
        }
        //PUT request to the given URL with optional body
        public override pillowsharp.Middleware.RestResponse Put(string Url, string Body = null, KeyValuePair<string, object>[] QueryParameter = null)
        {
            var putRequest = BuildRequestBase(Url, Method.Put, QueryParameter: QueryParameter);
            if (!string.IsNullOrEmpty(Body))
                AddJSONBody(putRequest, Body);
            return Request(putRequest);
        }

        //DELETE request to the given URL 
        public override Task<pillowsharp.Middleware.RestResponse> DeleteAsync(string Uri)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.Delete(Uri);
            });
        }
        public override pillowsharp.Middleware.RestResponse Delete(string Uri)
        {
            return Request(BuildRequestBase(Uri, Method.Delete));
        }


        public override Task<pillowsharp.Middleware.RestResponse> PostAsync(string Uri, string Body = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.Post(Uri, Body);
            });
        }
        //Post requrest to the given URL with an optional Body
        public override pillowsharp.Middleware.RestResponse Post(string Uri, string Body = null)
        {
            var postRequest = BuildRequestBase(Uri, Method.Post);
            if (!string.IsNullOrEmpty(Body))
                AddJSONBody(postRequest, Body);
            return Request(postRequest);
        }

        private RestRequest BuildFileBaseRequest(string ID, string AttachmentName, string Revision, string Database, Method Method)
        {
            AttachmentName = HttpUtility.UrlEncode(AttachmentName);
            var getRequest = BuildRequestBase($"{Database}/{ID}/{AttachmentName}", Method);
            getRequest.AddQueryParameter(Rev, Revision);
            return getRequest;
        }
        public override Task<pillowsharp.Middleware.RestResponse> UploadFileAsync(string ID, string AttachmentName, string Revision, string Database, string File)
        {
            return Task.Factory.StartNew(() =>
            {
                return UploadFile(ID, AttachmentName, Revision, Database, File);
            });
        }
        //Add a file to an existing document
        public override pillowsharp.Middleware.RestResponse UploadFile(string ID, string AttachmentName, string Revision, string Database, string File)
        {
            var putRequest = BuildFileBaseRequest(ID, AttachmentName, Revision, Database, Method.Put);
            var contentType = MimeMapping.GetMimeType(File);
            putRequest.AlwaysMultipartFormData = false;
            putRequest.AddHeader("Content-Type", contentType);
            putRequest.AddParameter(contentType, System.IO.File.ReadAllBytes(File), ParameterType.RequestBody);
            return Request(putRequest);
        }

        public override Task<pillowsharp.Middleware.RestResponse> DeleteFileAsync(string ID, string AttachmentName, string Revision, string Database)
        {
            return Task.Factory.StartNew(() =>
            {
                return DeleteFile(ID, AttachmentName, Revision, Database);
            });
        }
        //Delete a file from an existing document
        public override pillowsharp.Middleware.RestResponse DeleteFile(string ID, string AttachmentName, string Revision, string Database)
        {
            var deleteRequest = BuildFileBaseRequest(ID, AttachmentName, Revision, Database, Method.Delete);
            return Request(deleteRequest);
        }

        //Get a file from an existing document
        public override pillowsharp.Middleware.RestResponse GetFile(string ID, string AttachmentName, string Revision, string Database)
        {
            return Request(BuildFileBaseRequest(ID, AttachmentName, Revision, Database, Method.Get));
        }

        //Set the given value for the coo
        public override void SetCookie(string CookieName, string Value)
        {
            var currentCookie = client.Options.CookieContainer.GetCookies(serverURI)[CookieName];
            if (currentCookie != null)
            {
                currentCookie.Value = Value;
            }
            else
            {
                client.Options.CookieContainer.Add(serverURI, new Cookie(CookieName, Value, "/"));
            }
        }

        public override Task<pillowsharp.Middleware.RestResponse> ViewAsync(string Database, string DocumentName, string ViewFunctionName, KeyValuePair<string, object>[] QueryParameter, HttpRequestMethod HTTPMethod, string Filter, string partition = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return View(Database, DocumentName, ViewFunctionName, QueryParameter, HTTPMethod, Filter, partition);
            });
        }
        //Run, change or create the given view
        public override pillowsharp.Middleware.RestResponse View(string Database, string DocumentName, string ViewFunctionName, KeyValuePair<string, object>[] QueryParameter, HttpRequestMethod HTTPMethod, string Filter, string partition = null)
        {
            //Ensure we can match the "new" restsharp http methdo enum
            if (Enum.TryParse(HTTPMethod.ToString(), true, out Method targetHttpMethod) == false)
                throw new PillowException($"Unable to use {HTTPMethod} as a request method in the default WebRequestHelper");

            var urlParts = new List<string>(){
                Database
            };
            if (string.IsNullOrEmpty(partition) == false)
            {
                urlParts.Add(CouchEntryPoints.Partition);
                urlParts.Add(partition);
            }
            urlParts.Add(DocumentName);
            urlParts.Add(CouchEntryPoints.ViewDoc);
            urlParts.Add(ViewFunctionName);

            var viewRequest = BuildRequestBase(BuildURL(urlParts.ToArray()),
                                          QueryParameter: QueryParameter, Method: targetHttpMethod);
            if (!string.IsNullOrEmpty(Filter))
                AddJSONBody(viewRequest, Filter);
            return Request(viewRequest);
        }

        public override pillowsharp.Middleware.RestResponse List(string database, string designDocumentId, string listName, string ViewName, KeyValuePair<string, object>[] queryParameter)
        {
            var listRequest = BuildRequestBase(BuildURL(database, designDocumentId, CouchEntryPoints.List, listName, ViewName), (Method)HttpRequestMethod.GET, QueryParameter: queryParameter);
            return Request(listRequest);
        }

        public override Task<pillowsharp.Middleware.RestResponse> HeadAsync(string Uri)
        {
            return Task.Factory.StartNew(() =>
            {
                return Head(Uri);
            });
        }
        public override pillowsharp.Middleware.RestResponse Head(string Uri)
        {
            return Request(BuildRequestBase(Uri, Method.Head));
        }

        public override Task<pillowsharp.Middleware.RestResponse> GetAsync(string Url)
        {
            return Task.Factory.StartNew(() =>
            {
                return this.Get(Url);
            });
        }

        public override Task<pillowsharp.Middleware.RestResponse> GetFileAsync(string ID, string AttachmentName, string Revision, string Database)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetFile(ID, AttachmentName, Revision, Database);
            });
        }
    }
}