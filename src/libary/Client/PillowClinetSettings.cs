using PillowSharp.Middleware;
using PillowSharp.Middleware.Default;
using PillowSharp.CouchType;
using PillowSharp.BaseObject;


using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using RestSharp;
using System.Linq;
using System.IO;
using PillowSharp.Helper;
using System.Net;

namespace PillowSharp.Client
{
    public class PillowClientSettings
    {
        public bool AutoGenerateID { get; set; } = true;

        public bool UseCouchUUID { get; set; } = false;

        public bool IgnoreJSONNull { get; set; } = true;

        
    }
}