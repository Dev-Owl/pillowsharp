using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchDocumentChange : CouchConfirm
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonProperty("rev")]
        [JsonPropertyName("rev")]
        public string Rev { get; set; }

        public CouchDocument ToCouchDocument()
        {
            return new CouchDocument() { ID = this.ID, Rev = this.Rev, Deleted = false };
        }
    }
}