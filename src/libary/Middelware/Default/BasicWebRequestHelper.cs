using System;
using System.Net;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using RestSharp;
using RestSharp.Authenticators;
using PillowSharp.Helper;
using System.Linq;
using PillowSharp.Client;

namespace PillowSharp.Middelware.Default 
{
    public class BasicWebRequestHelper : AWebRequestHelper
    {
        //RestSharp client
        RestClient client = null;
        //Coockie storage for autehntication, optional
        CookieContainer cookieContainer = null;
        //URI to server, used for coockie
        Uri serverURI = null;

        //Create the middelware and pass server data to it
        public BasicWebRequestHelper(ICouchdbServer Server):base(Server)
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
        
        private RestRequest BuildRequestBase(string Resource,Method Method = Method.GET,int TimeOut = 30){
            var request = new RestRequest();
            request.Method = Method;
            request.Timeout = TimeOut;
            request.Resource = Resource;
            return request;
        }
        private RestRequest AddJSONBody(RestRequest request,string Body){
            //add a json body to the request
            request.AddParameter("application/json",Body,ParameterType.RequestBody);
            return request;
        }

        //Base function for all requests
        private Task<IRestResponse> Request(RestRequest Request){
            return client.GetResponseAsync(Request);
        }

        //Get a single document with the given revesion number from the db
        public override Task<IRestResponse> GetDocument(string ID, string Database, string Revision = null)
        {
            var getRequest = BuildRequestBase($"{Database}/{ID}");
            if(!string.IsNullOrEmpty(Revision))
                getRequest.AddQueryParameter(Rev,Revision);
            return Request(getRequest); //TODO check if really not yet excuted
        }
        public override Task<IRestResponse> Get(string Url)
        {
           return Request(BuildRequestBase(Url));
        }

      
        public override Task<IRestResponse> Put(string Url, string Body = null)
        {
            var putRequest = BuildRequestBase(Url,Method.PUT);
            if(!string.IsNullOrEmpty(Body))
                AddJSONBody(putRequest,Body);
            return Request(putRequest);
        }

        public override Task<IRestResponse> Delete(string Uri)
        {
            throw new NotImplementedException();
        }

        public override Task<IRestResponse> Post(string Uri, string Body = null)
        {
             var postRequest = BuildRequestBase(Uri,Method.POST);
            if(!string.IsNullOrEmpty(Body))
                AddJSONBody(postRequest,Body);
            return Request(postRequest);
        }

        public override void SetCoockie(string CoockieName, string Value)
        {
            var currentCoockie = client.CookieContainer.GetCookies(serverURI)[CoockieName];
            if(currentCoockie != null){
                currentCoockie.Value = Value;
            }
            else{
                client.CookieContainer.Add(serverURI,new Cookie(CoockieName,Value,"/"));
            }
        }
    }
}