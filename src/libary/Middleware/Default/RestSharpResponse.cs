﻿using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace pillowsharp.Middleware.Default
{
    public class RestSharpResponse : RestResponse
    {

        private RestSharp.RestResponse restSharpResponse = null;

        public RestSharpResponse(RestSharp.RestResponse Response)
        {
            restSharpResponse = Response;
            this.Content = restSharpResponse.Content;
            this.ResponseCode = restSharpResponse.StatusCode;
            this.RawBytes = restSharpResponse.RawBytes;
            var cookies = new List<SimpleCookie>();
            if (Response.Cookies != null)
            {
                foreach (System.Net.Cookie restCookie in Response.Cookies)
                {
                    cookies.Add(new SimpleCookie() { Name = restCookie.Name, Value = restCookie.Value });
                }
            }

            this.Cookies = cookies;
            this.Header = new List<KeyValuePair<string, string>>();
            if (Response.Headers != null)
            {
                foreach (var header in Response.Headers.Where(h => h.Type == ParameterType.HttpHeader))
                {
                    this.Header.Add(new KeyValuePair<string, string>(header.Name, header.Value?.ToString()));
                }
            }

            this.ContentHeader = new List<KeyValuePair<string, string>>();

            if (Response.ContentHeaders != null)
            {
                foreach (var header in Response.ContentHeaders.Where(h => h.Type == ParameterType.HttpHeader))
                {
                    if (this.ContentHeader.Any(h => h.Key == header.Name)) continue;
                    this.ContentHeader.Add(new KeyValuePair<string, string>(header.Name, header.Value?.ToString()));
                }
            }


        }
    }
}
