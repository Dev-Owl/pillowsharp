using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchLoginResponse:CouchConfirm
    {
        [JsonProperty("name")]
        public string Name { get; set; }        
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }
}