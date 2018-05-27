using System;
using System.Net;
using System.Threading.Tasks;
using pillowsharp.Middleware;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using PillowSharp.Middleware;

namespace PillowSharp.Helper
{
    public static class PillowErrorHelper
    {
        public static void HandleNoneOKResponse(RestResponse Response,IJSONHelper JsonHelper)
        {
            CouchError error = null;
            if((Response.ResponseCode & (HttpStatusCode.InternalServerError | HttpStatusCode.NotFound | HttpStatusCode.Unauthorized))> 0){
                error = JsonHelper.FromJSON<CouchError>(Response.Content);
                if(error != null)
                    error.HTTPCode = (int)Response.ResponseCode;
                else
                    throw new PillowException($"Error in Couch response, unable to parse error info. Raw:{Response.Content}");
            }
            else{
                
                error = new CouchError(){
                    Error = "Generic",
                    HTTPCode = (int)Response.ResponseCode,
                    Reason ="Error, see status code"
                };
            }
            throw new CouchException(error);
        
        }
    }
}