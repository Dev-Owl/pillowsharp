using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchBulk<T> where T:CouchDocument
    {
        public List<T> docs { get; set; }

        public CouchBulk()
        {
            docs = new List<T>();
        }

        public CouchBulk(T Document)
        {
            docs = new List<T>(){Document};
        }

        public CouchBulk(List<T> Documents)
        {
            docs = Documents;
        }
    }
}