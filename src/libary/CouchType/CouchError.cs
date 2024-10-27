using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchError
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int HTTPCode { get; set; }
        [JsonProperty("error")]
        [JsonPropertyName("error")]

        public string Error { get; set; }
        [JsonProperty("reason")]
        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        public override string ToString()
        {
            return Reason;
        }
    }
}