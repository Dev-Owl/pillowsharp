using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PillowSharp.BaseObject;
using RestSharp;
using System.Net;
using PillowSharp.CouchType;

namespace PillowSharp.Middelware.Default 
{
    public class BasicJSONHelper : IJSONHelper
    {
        public T FromJSON<T>(string JSON) where T : new()
        {
            throw new NotImplementedException();
        }

        public T FromJSON<T>(IRestResponse Response) where T : new()
        {
            CouchError error = null;
            if(Response.StatusCode == HttpStatusCode.OK || Response.StatusCode == HttpStatusCode.Created){
                //Get the normal object as requested
                return (T)JsonConvert.DeserializeObject(Response.Content,typeof(T));
            }
            else if((Response.StatusCode & (HttpStatusCode.InternalServerError | HttpStatusCode.NotFound | HttpStatusCode.Unauthorized))> 0){
                error = JsonConvert.DeserializeObject<CouchError>(Response.Content);
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

        public string ToJSON(object Data)
        {
            return JsonConvert.SerializeObject(Data);
        }
    }
}