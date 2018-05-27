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

        //Create the Middleware and pass server data to it
        public BasicWebRequestHelper(ICouchdbServer Server):base(Server)
        {
            UpdateServerData(Server);
        }

        public override void UpdateServerData(ICouchdbServer Server)
        {
            //Creteate the rest clinet
            client = new RestClient(Server.GetServerURL());
            //Generate needed data for selected auth type
            if(Server.LoginType == ELoginTypes.BasicAuth)
            {
                client.Authenticator = new HttpBasicAuthenticator(Server.GetLoginData().UserName, Server.GetLoginData().Password);
            }
            else if(Server.LoginType == ELoginTypes.TokenLogin)
            {
                cookieContainer = new CookieContainer();
                client.CookieContainer = cookieContainer;
                serverURI = new Uri(Server.GetServerURL());
            }
        }

        private RestRequest BuildRequestBase(string Resource,Method Method = Method.GET,int TimeOut = 30,KeyValuePair<string,object>[] QueryParameter = null){
            var request = new RestRequest
            {
                Method = Method,
                Timeout = TimeOut * 1000,
                Resource = Resource
            };
            if (QueryParameter != null){
                foreach(var keyValue in QueryParameter){
                    if(keyValue.Key != null && keyValue.Value != null)
                        request.AddQueryParameter(keyValue.Key,keyValue.Value.ToString());
                }
            }
            return request;
        }
        private RestRequest AddJSONBody(RestRequest request,string Body){
            //add a json body to the request
            request.AddParameter("application/json",Body,ParameterType.RequestBody); //todo add type as constant
            return request;
        }

        //Base function for all requests
        private Task<pillowsharp.Middleware.RestResponse> Request(RestRequest Request){
            
            return client.GetResponseAsync(Request).Then(Response => new RestSharpResponse(Response) as pillowsharp.Middleware.RestResponse);
        }

        //Get a single document with the given revesion number from the db
        public override Task<pillowsharp.Middleware.RestResponse> GetDocument(string ID, string Database, string Revision = null)
        {
           return Get(BuildURL(Database,ID),new KeyValuePair<string, object>(Rev,Revision)); 
        }
        
        //Get request to the given URL
        public override Task<pillowsharp.Middleware.RestResponse> Get(string Url)
        {
           return Request(BuildRequestBase(Url));
        }
        //Get request to the given URL include Request parameters
        public override Task<pillowsharp.Middleware.RestResponse> Get(string Url, params KeyValuePair<string,object>[] Parameter)
        {
            var getRequest = BuildRequestBase(Url);
            if(Parameter != null)
            {
                foreach(var keyValue in Parameter)
                {
                    if(keyValue.Key != null && keyValue.Value != null)
                        getRequest.AddQueryParameter(keyValue.Key,keyValue.Value.ToString());
                }
            }
            return Request(getRequest);
        }

        //PUT request to the given URL with optional body
        public override Task<pillowsharp.Middleware.RestResponse> Put(string Url, string Body = null)
        {
            var putRequest = BuildRequestBase(Url,Method.PUT);
            if(!string.IsNullOrEmpty(Body))
                AddJSONBody(putRequest,Body);
            return Request(putRequest);
        }
        
        //DELETE request to the given URL 
        public override Task<pillowsharp.Middleware.RestResponse> Delete(string Uri)
        {
            return Request( BuildRequestBase(Uri,Method.DELETE));
        }
        //Post requrest to the given URL with an optional Body
        public override Task<pillowsharp.Middleware.RestResponse> Post(string Uri, string Body = null)
        {
             var postRequest = BuildRequestBase(Uri,Method.POST);
            if(!string.IsNullOrEmpty(Body))
                AddJSONBody(postRequest,Body);
            return Request(postRequest);
        }

        private RestRequest BuildFileBaseRequest(string ID, string AttachmentName, string Revision, string Database,Method Method)
        {
            AttachmentName = HttpUtility.UrlEncode(AttachmentName);
            var getRequest = BuildRequestBase($"{Database}/{ID}/{AttachmentName}",Method);
            getRequest.AddQueryParameter(Rev,Revision);
            return getRequest;
        }
        //Add a file to an existing document
        public override Task<pillowsharp.Middleware.RestResponse> UploadFile(string ID,string AttachmentName,string Revision,string Database,string File)
        {
            var putRequest = BuildFileBaseRequest(ID,AttachmentName,Revision,Database,Method.PUT);
            var contentType = MimeMapping.GetMimeType(File);
            putRequest.AlwaysMultipartFormData = false;
            putRequest.AddHeader("Content-Type",contentType);
            putRequest.AddParameter(contentType, System.IO.File.ReadAllBytes(File), ParameterType.RequestBody);
            return Request(putRequest);
        }
        //Delete a file from an existing document
        public override Task<pillowsharp.Middleware.RestResponse> DeleteFile(string ID, string AttachmentName, string Revision, string Database)
        {
            var deleteRequest = BuildFileBaseRequest(ID,AttachmentName,Revision,Database,Method.DELETE);
            return Request(deleteRequest);
        }

        //Get a file from an existing document
        public override Task<pillowsharp.Middleware.RestResponse> GetFile(string ID, string AttachmentName, string Revision, string Database)
        {
            return Request(BuildFileBaseRequest(ID,AttachmentName,Revision,Database,Method.GET));
        }

        //Set the given value for the coo
        public override void SetCookie(string CookieName, string Value)
        {
            var currentCookie = client.CookieContainer.GetCookies(serverURI)[CookieName];
            if(currentCookie != null){
                currentCookie.Value = Value;
            }
            else{
                client.CookieContainer.Add(serverURI,new Cookie(CookieName,Value,"/"));
            }
        }      

        //Run, change or create the given view
        public override Task<pillowsharp.Middleware.RestResponse> View(string Database, string DocumentName, string ViewFunctionName, KeyValuePair<string, object>[] QueryParameter,HttpRequestMethod HTTPMethod,string Filter)
        {
            if (!Enum.IsDefined(typeof(Method), HTTPMethod.ToString()))
                throw new PillowException($"Unable to use {HTTPMethod} as a request method in the default WebRequestHelper");
            var viewRequest = BuildRequestBase(BuildURL(Database,DocumentName,CouchEntryPoints.ViewDoc,ViewFunctionName),
                                          QueryParameter:QueryParameter,Method: (Method)HTTPMethod);
            if(!string.IsNullOrEmpty(Filter))
                AddJSONBody(viewRequest,Filter);
            return Request(viewRequest);
        }

    }
}