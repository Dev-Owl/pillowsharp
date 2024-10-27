using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchViewFilter
    {
        [JsonProperty("keys")]
        [JsonPropertyName("keys")]
        public List<string> Keys { get; set; }

        public CouchViewFilter(List<string> ViewFilterByID)
        {
            Keys = ViewFilterByID;
        }
    }
}