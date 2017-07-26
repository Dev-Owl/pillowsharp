using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchDocument
    {
        public string _id { get; set; }  = null;

        public string _rev { get; set; } = null;

        public bool _deleted { get; set; } = false;
    }
}