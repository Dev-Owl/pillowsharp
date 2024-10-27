using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchViewResponse<T>
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonProperty("key")]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public T Value { get; set; }
    }
}