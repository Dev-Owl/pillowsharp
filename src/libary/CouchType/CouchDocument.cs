using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchDocument
    {
        public string _id { get; set; }

        public string _rev { get; set; }

       
        public bool _deleted {get;set;}
    }
}