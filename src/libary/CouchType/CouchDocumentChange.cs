using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchDocumentChange : CouchConfirm
    {
          [JsonProperty("id")]
          public string ID { get; set; }                   

          [JsonProperty("rev")]
          public string Rev {get; set;}

          public CouchDocument ToCouchDocument(){
              return new CouchDocument(){ID = this.ID,Rev = this.Rev,Deleted=false};
          }
    }
}