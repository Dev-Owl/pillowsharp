
using System;
using System.Net;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using PillowSharp.Middelware;
using RestSharp;

namespace PillowSharp.Helper
{
    public static class PillowErrorHelper
    {
        public static void HandleNoneOKResponse(IRestResponse Response,IJSONHelper JsonHelper)
        {
            CouchError error = null;
            if((Response.StatusCode & (HttpStatusCode.InternalServerError | HttpStatusCode.NotFound | HttpStatusCode.Unauthorized))> 0){
                error = JsonHelper.FromJSON<CouchError>(Response.Content);
                if(error != null)
                    error.HTTPCode = (int)Response.StatusCode;
                else
                    throw new PillowException($"Error in Couch response, unable to parse error info. Raw:{Response.Content}");
            }
            else{
                
                error = new CouchError(){
                    error = "Generic",
                    HTTPCode = (int)Response.StatusCode,
                    reason ="Error, see status code"
                };
            }
            throw new CouchException(error);
        
        }
    }
}