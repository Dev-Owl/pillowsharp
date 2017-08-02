using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchUUIDResponse
    {
        [JsonProperty("uuids")]
        public List<string> UUIDS { get; set; }
    }
}