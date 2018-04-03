using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace pillowsharp.Middelware.Default
{
    public class RestSharpResponse : RestResponse
    {

        private IRestResponse restSharpResponse = null;

        public RestSharpResponse(IRestResponse Response)
        {
            restSharpResponse = Response;
            this.Content = restSharpResponse.Content;
            this.ResponseCode = restSharpResponse.StatusCode;
            this.RawBytes = restSharpResponse.RawBytes;
            var cookies = new List<SimpleCookie>();
            foreach(var restCookie in Response.Cookies)
            {
                cookies.Add(new SimpleCookie() { Name = restCookie.Name, Value = restCookie.Value });
            }
            this.Cookies = cookies;
        }
    }
}
