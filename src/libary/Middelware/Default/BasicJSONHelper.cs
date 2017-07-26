using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PillowSharp.BaseObject;
using RestSharp;
using System.Net;
using PillowSharp.CouchType;
using PillowSharp.Helper;

namespace PillowSharp.Middelware.Default 
{
    public class BasicJSONHelper : IJSONHelper
    {

        public BasicJSONHelper(bool IgnoreNull = true)
        {
            if(IgnoreNull)
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        }

        public T FromJSON<T>(string JSON) where T : new()
        {
            return (T)JsonConvert.DeserializeObject(JSON,typeof(T));
        }

        public T FromJSON<T>(IRestResponse Response) where T : new()
        {
            if(Response.StatusCode == HttpStatusCode.OK || Response.StatusCode == HttpStatusCode.Created){
                //Get the normal object as requested
                return FromJSON<T>(Response.Content);
            }
            else
            {
                PillowErrorHelper.HandleNoneOKResponse(Response,this);
                return default(T); //Will not be reached the above function will throw an error
            }
        }

        public string ToJSON(object Data)
        {
            return JsonConvert.SerializeObject(Data);
        }
    }
}