using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using RestSharp;

namespace PillowSharp.Middelware
{
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
        public abstract void SetCoockie(string CoockieName,string Value);

        
        public abstract Task<IRestResponse> GetDocument(string ID,string Database,string Revision=null);
        
        public abstract Task<IRestResponse> Get(string Url);

        public abstract Task<IRestResponse> Get(string Url,params KeyValuePair<string,object>[] Parameter);
        
        public abstract Task<IRestResponse> Put(string Url,string Body=null);

        public abstract Task<IRestResponse> Delete(string Uri);

        public abstract Task<IRestResponse> Post(string Uri,string Body = null);

        public abstract Task<IRestResponse> UploadFile(string ID,string AttachmentName,string Revision,string Database,string File);

        public abstract Task<IRestResponse> DeleteFile(string ID,string AttachmentName,string Revision,string Database);

        public abstract Task<IRestResponse> GetFile(string ID,string AttachmentName,string Revision,string Database);


    }
}