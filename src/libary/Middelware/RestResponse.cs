using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace pillowsharp.Middelware
{
    public abstract class RestResponse
    {
        public HttpStatusCode ResponseCode { get; internal set; }

        public string Content { get; internal set; }

        public byte[] RawBytes { get; internal set; }

        public IEnumerable<SimpleCookie> Cookies { get; internal set; }

    }

    public class SimpleCookie
    {
        public string Name { get; internal set; }

        public string Value { get; internal set; }
    }

    
}
