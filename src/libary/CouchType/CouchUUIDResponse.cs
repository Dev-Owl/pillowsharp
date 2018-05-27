using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchUUIDResponse
    {
        [JsonProperty("uuids")]
        public List<string> UUIDS { get; set; }
    }
}