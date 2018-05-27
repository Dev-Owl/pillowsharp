using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchConfirm
    {
      [JsonProperty("ok")]
      public bool Ok { get; set; }           
    }
}