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

        
        public abstract Task<RestResponse> GetDocument(string ID,string Database,string Revision=null);
        
        public abstract Task<RestResponse> Get(string Url);

        public abstract Task<RestResponse> Get(string Url,params KeyValuePair<string,object>[] Parameter);
        
        public abstract Task<RestResponse> Put(string Url,string Body=null);

        public abstract Task<RestResponse> Delete(string Uri);

        public abstract Task<RestResponse> Post(string Uri,string Body = null);

        public abstract Task<RestResponse> UploadFile(string ID,string AttachmentName,string Revision,string Database,string File);

        public abstract Task<RestResponse> DeleteFile(string ID,string AttachmentName,string Revision,string Database);

        public abstract Task<RestResponse> GetFile(string ID,string AttachmentName,string Revision,string Database);

        public abstract Task<RestResponse> View(string Database,string DocumentName,string ViewFunctionName,KeyValuePair<string,object>[] QueryParameter, HttpRequestMethod HTTPMethod,string Filter);

        public virtual string BuildURL(params string[] URL)
        {
            return string.Join("/", URL);
        }

    }
}