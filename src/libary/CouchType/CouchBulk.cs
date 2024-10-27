using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchBulk<T> where T : CouchDocument
    {
        [JsonProperty("docs")]
        [JsonPropertyName("docs")]
        public List<T> Docs { get; set; }

        public CouchBulk()
        {
            Docs = new List<T>();
        }

        public CouchBulk(T Document)
        {
            Docs = new List<T>() { Document };
        }

        public CouchBulk(List<T> Documents)
        {
            Docs = Documents;
        }
    }
}