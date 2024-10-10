using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PillowSharp.Middleware;
using PillowSharp.BaseObject;

namespace PillowSharp.CouchType
{
    public class MangoIndex
    {
        [JsonProperty(PropertyName = "index")]
        public MangoIndexFields Index { get; set; }

        [JsonProperty(PropertyName = "ddoc")]
        public string DesignDocument { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "json";

        [JsonProperty(PropertyName = "partial_filter_selector")]
        public MangoSelector PartialSelector { get; set; }

        [JsonProperty(PropertyName = "partitioned")]
        public bool? Partitioned { get; set; }
    }

    public class MangoIndexFields
    {
        [JsonProperty(PropertyName = "fields")]
        public List<string> Fields { get; set; }
    }

    public class MangoIndexResponse
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}