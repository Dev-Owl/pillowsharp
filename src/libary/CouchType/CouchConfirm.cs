using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchConfirm
    {
      [JsonProperty("ok")]
      public bool Ok { get; set; }           
    }
}