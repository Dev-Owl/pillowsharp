using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchLoginResponse:CouchConfirm
    {
        public string name { get; set; }        
        public List<string> roles { get; set; }
    }
}