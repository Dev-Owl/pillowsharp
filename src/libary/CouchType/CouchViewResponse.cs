using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchViewResponse<T> 
    {
        public string id { get; set; }

        public string key { get; set; } 

        public T value { get; set; } 
    }
}