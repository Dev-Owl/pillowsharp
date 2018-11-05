using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using System.Linq;
using pillowsharp.Middleware;

namespace PillowSharp.Middleware
{
    //TODO Remove Related to RestSharp from here
    public abstract class AWebRequestHelper
    {

        //Defaults for query parameter
        public const string Rev ="rev";

        internal ICouchdbServer _server = null;

        public AWebRequestHelper(ICouchdbServer Server)
        {
            this._server = Server;
        }

        public abstract void UpdateServerData(ICouchdbServer Server);
        public abstract void SetCookie(string CookieName,string Value);

        
        public abstract Task<RestResponse> GetDocumentAsync(string ID,string Database,string Revision=null);
        
        public abstract Task<RestResponse> GetAsync(string Url);

        public abstract Task<RestResponse> GetAsync(string Url,params KeyValuePair<string,object>[] Parameter);
        
        public abstract Task<RestResponse> PutAsync(string Url,string Body=null,KeyValuePair<string,object>[] QueryParameter= null);

        public abstract Task<RestResponse> DeleteAsync(string Uri);

        public abstract Task<RestResponse> PostAsync(string Uri,string Body = null);

        public abstract Task<RestResponse> UploadFileAsync(string ID,string AttachmentName,string Revision,string Database,string File);

        public abstract Task<RestResponse> DeleteFileAsync(string ID,string AttachmentName,string Revision,string Database);

        public abstract Task<RestResponse> GetFileAsync(string ID,string AttachmentName,string Revision,string Database);

        public abstract Task<RestResponse> ViewAsync(string Database,string DocumentName,string ViewFunctionName,KeyValuePair<string,object>[] QueryParameter, HttpRequestMethod HTTPMethod,string Filter);

        public abstract Task<RestResponse> HeadAsync(string Uri);

        public abstract RestResponse GetDocument(string ID,string Database,string Revision=null);
        
        public abstract RestResponse Get(string Url);

        public abstract RestResponse Get(string Url,params KeyValuePair<string,object>[] Parameter);
        
        public abstract RestResponse Put(string Url,string Body=null,KeyValuePair<string,object>[] QueryParameter= null);

        public abstract RestResponse Delete(string Uri);

        public abstract RestResponse Post(string Uri,string Body = null);

        public abstract RestResponse UploadFile(string ID,string AttachmentName,string Revision,string Database,string File);

        public abstract RestResponse DeleteFile(string ID,string AttachmentName,string Revision,string Database);

        public abstract RestResponse GetFile(string ID,string AttachmentName,string Revision,string Database);

        public abstract RestResponse View(string Database,string DocumentName,string ViewFunctionName,KeyValuePair<string,object>[] QueryParameter, HttpRequestMethod HTTPMethod,string Filter);

        public abstract RestResponse Head(string Uri);


        public virtual string BuildURL(params string[] URL)
        {
            return string.Join("/", URL);
        }

    }
}