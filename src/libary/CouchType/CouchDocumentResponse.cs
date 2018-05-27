using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchDocumentResponse<T> where T : new()
    {
        [JsonProperty("total_rows")]
        public int? TotalRows { get; set; }
        [JsonProperty("offset")]
        public int? Offset { get; set; }
        [JsonProperty("update_seq")]
        public int? UpdateSeq  { get; set; }
        [JsonProperty("rows")]
        public List<T> Rows { get; set; }             
    }
}