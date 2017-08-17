using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchViewRow<TKey,TValue>
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("key")]
        public TKey Key { get; set; }

        [JsonProperty("value")]
        public TValue Value { get; set; }
    }

}