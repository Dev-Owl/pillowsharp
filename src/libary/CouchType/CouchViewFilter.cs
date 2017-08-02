using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchViewFilter
    {
        public List<string> keys { get; set; }
        public CouchViewFilter(List<string> ViewFilterByID)
        {
            keys = ViewFilterByID;
        }
    }
}