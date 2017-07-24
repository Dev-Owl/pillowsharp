using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchUUIDResponse
    {
        public List<string> uuids { get; set; }
    }
}