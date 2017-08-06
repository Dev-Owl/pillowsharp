using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PillowSharp.Helper;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchDesignDocument : CouchDocument
    {

        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("views")]
        public Dictionary<string,CouchView> Views { get; set; }

        [JsonProperty("shows")]
        public Dictionary<string,string> Shows {get;set;}

        [JsonProperty("filters")]
        public Dictionary<string,string> Filters {get;set;}

        [JsonProperty("lists")]
        public Dictionary<string,string> Lists {get;set;}

        [JsonProperty("updates")]
        public Dictionary<string,string> Updates {get;set;}


        public CouchDesignDocument(string Language ="javascript")
        {
          Init(Language);
        }

        public CouchDesignDocument()
        {
            Init();
        }

        private void Init(string Language ="javascript")
        {
            Views   = new Dictionary<string,CouchView>();
            Shows   = new Dictionary<string, string>();
            Filters = new Dictionary<string, string>();
            Lists   = new Dictionary<string, string>();
            Updates = new Dictionary<string, string>();
            this.Language = Language;
        }

        private void EnsureExists<TValue>(Dictionary<string,TValue> Dictionary,string Key,TValue Value = default(TValue)) 
        {
            if(!Dictionary.ContainsKey(Key))
                Dictionary.Add(Key,Value);
        }

        public void AddView(string Name,string Map, string Reduce=null)
        {  
            EnsureExists(Views,Name,new CouchView());
            var view = Views[Name];
            view.Map= Map;
            view.Reduce = Reduce;
        }

        public void AddShow(string Name, string Function)
        {
            EnsureExists(Shows,Name,Function);
            Shows[Name] = Function;
            
        }

        public void AddFilter(string Name, string Function)
        {
            EnsureExists(Filters,Name,Function);
            Filters[Name] = Function;
        }

        public void AddList(string Name, string Function)
        {
            EnsureExists(Lists,Name,Function);
            Lists[Name] = Function;
        }

         public void AddUpdate(string Name, string Function)
        {
            EnsureExists(Updates,Name,Function);
            Updates[Name] = Function;
        }

    }

    public class CouchView
    {
        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("reduce")]
        public string Reduce {get;set;}

    }

  
}