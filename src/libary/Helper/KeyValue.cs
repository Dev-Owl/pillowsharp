
using System;
using System.Net;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using PillowSharp.Middleware;
using RestSharp;

namespace PillowSharp.Helper
{
    public class KeyValue<TKey,TValue>
    {

        public KeyValue(TKey Key,TValue Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        public KeyValue()
        {
            
        }

        public TKey Key { get; set; }

        public TValue Value { get; set; }
    }
}