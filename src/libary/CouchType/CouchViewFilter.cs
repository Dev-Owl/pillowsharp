using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchViewFilter
    {
        [JsonProperty("keys")]
        public List<string> Keys { get; set; }

        public CouchViewFilter(List<string> ViewFilterByID)
        {
            Keys = ViewFilterByID;
        }
    }
}