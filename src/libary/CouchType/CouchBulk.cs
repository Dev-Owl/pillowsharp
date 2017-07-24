using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchBulk
    {
        public List<CouchDocument> docs { get; set; }

        public CouchBulk()
        {
            docs = new List<CouchDocument>();
        }

        public CouchBulk(CouchDocument Document)
        {
            docs = new List<CouchDocument>(){Document};
        }

        public CouchBulk(List<CouchDocument> Documents)
        {
            docs = Documents;
        }
    }
}