using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchError
    {
        [JsonIgnore]
        public int HTTPCode { get; set; }

        public string error { get; set; }

        public string reason { get; set; } 
    }
}