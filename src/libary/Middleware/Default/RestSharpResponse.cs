using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace pillowsharp.Middleware.Default
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
            this.Header = new List<KeyValuePair<string, string>>();
            foreach (var header in Response.Headers.Where(h => h.Type == ParameterType.HttpHeader))
            {
                this.Header.Add(new KeyValuePair<string, string>(header.Name, header.Value?.ToString()));
            }

        }
    }
}
