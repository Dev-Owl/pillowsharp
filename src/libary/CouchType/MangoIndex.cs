using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PillowSharp.Middleware;
using PillowSharp.BaseObject;
using System.Text.Json.Serialization;

namespace PillowSharp.CouchType
{
    public class MangoIndex
    {
        [JsonProperty(PropertyName = "index")]
        [JsonPropertyName("index")]
        public MangoIndexFields Index { get; set; }

        [JsonProperty(PropertyName = "ddoc")]
        [JsonPropertyName("ddoc")]
        public string DesignDocument { get; set; }

        [JsonProperty(PropertyName = "name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        [JsonPropertyName("type")]
        public string Type { get; set; } = "json";

        [JsonProperty(PropertyName = "partial_filter_selector")]
        [JsonPropertyName("partial_filter_selector")]
        public MangoSelector PartialSelector { get; set; }

        [JsonProperty(PropertyName = "partitioned")]
        [JsonPropertyName("partitioned")]
        public bool? Partitioned { get; set; }
    }

    public class MangoIndexFields
    {
        [JsonProperty(PropertyName = "fields")]
        [JsonPropertyName("fields")]
        public List<string> Fields { get; set; }
    }

    public class MangoIndexResponse
    {
        [JsonProperty(PropertyName = "result")]
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}