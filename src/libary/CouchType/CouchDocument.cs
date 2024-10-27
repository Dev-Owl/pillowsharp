using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchDocument
    {
        [JsonProperty("_id")]
        [JsonPropertyName("_id")]
        public string ID { get; set; } = null;

        [JsonProperty("_rev")]
        [JsonPropertyName("_rev")]
        public string Rev { get; set; } = null;

        [JsonProperty("_deleted")]
        [JsonPropertyName("_deleted")]
        public bool Deleted { get; set; } = false;

        [JsonProperty("_attachments")]
        [JsonPropertyName("_attachments")]
        public Dictionary<string, CouchAttachment> Attachments { get; set; } = null;
    }
}