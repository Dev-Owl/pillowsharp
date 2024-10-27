using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchLoginResponse : CouchConfirm
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonProperty("roles")]
        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; }
    }
}