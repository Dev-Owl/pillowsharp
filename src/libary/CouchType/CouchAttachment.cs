using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PillowSharp.CouchType
{
    public class CouchAttachment
    {
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("revpos")]
        public string Revpos { get; set; }

        [JsonProperty("digest")]
        public string Digest { get; set; }

        [JsonProperty("length")]
        public Int64 Length { get; set; }

        [JsonProperty("stub")]
        public bool Stub { get; set; }
    }
}
