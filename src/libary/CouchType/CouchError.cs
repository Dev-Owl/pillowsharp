using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchError
    {
        [JsonIgnore]
        public int HTTPCode { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("reason")]
        public string Reason { get; set; } 
    }
}