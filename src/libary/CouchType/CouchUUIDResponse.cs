using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchUUIDResponse
    {
        [JsonProperty("uuids")]
        [JsonPropertyName("uuids")]
        public List<string> UUIDS { get; set; }
    }
}