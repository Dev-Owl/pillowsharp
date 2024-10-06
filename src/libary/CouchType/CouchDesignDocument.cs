using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PillowSharp.Helper;
using PillowSharp.Middleware;

namespace PillowSharp.CouchType
{
    public class CouchDesignDocument : CouchDocument
    {

        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("views")]
        public Dictionary<string, CouchView> Views { get; set; }

        [JsonProperty("shows")]
        public Dictionary<string, string> Shows { get; set; }

        [JsonProperty("filters")]
        public Dictionary<string, string> Filters { get; set; }

        [JsonProperty("lists")]
        public Dictionary<string, string> Lists { get; set; }

        [JsonProperty("updates")]
        public Dictionary<string, string> Updates { get; set; }

        [JsonProperty("options")]
        public Dictionary<string, object> Options { get; set; }

        public CouchDesignDocument(string Language = "javascript", bool Partitioned = false)
        {
            Init(Language, Partitioned);
        }

        public CouchDesignDocument()
        {
            Init();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CouchDesignDocument"/> class with the specified language and partitioned state.
        /// Toggles the partitioned state of the design document.
        /// </summary>
        /// <remarks>
        /// If the <c>Options</c> dictionary is null, it initializes it. 
        /// If the dictionary contains the key "partitioned", it toggles its boolean value.
        /// Otherwise, it adds the key "partitioned" with a value of <c>true</c>.
        /// </remarks>
        public void TogglePartitionedState()
        {
            if (Options == null)
            {
                Options = new Dictionary<string, object>();
            }

            if (Options.ContainsKey("partitioned"))
            {
                Options["partitioned"] = !(bool)Options["partitioned"];
            }
            else
            {
                Options.Add("partitioned", true);
            }
        }

        private void Init(string Language = "javascript", bool Partitioned = false)
        {
            Views = new Dictionary<string, CouchView>();
            Shows = new Dictionary<string, string>();
            Filters = new Dictionary<string, string>();
            Lists = new Dictionary<string, string>();
            Updates = new Dictionary<string, string>();
            this.Language = Language;
            if (Partitioned)
            {
                // init dictionary and add a partitioned key
                Options = new Dictionary<string, object>()  {
                    { "partitioned", true }
                };
            }
        }

        private void EnsureExists<TValue>(Dictionary<string, TValue> Dictionary, string Key, TValue Value = default(TValue))
        {
            if (!Dictionary.ContainsKey(Key))
                Dictionary.Add(Key, Value);
        }


        /// <summary>
        /// Adds a view to the design document.
        /// </summary>
        /// <param name="Name">The name of the view.</param>
        /// <param name="Map">The map function for the view.</param>
        /// <param name="Reduce">The reduce function for the view (optional).</param>
        public void AddView(string Name, string Map, string Reduce = null)
        {
            EnsureExists(Views, Name, new CouchView());
            var view = Views[Name];
            view.Map = Map;
            view.Reduce = Reduce;
        }

        /// <summary>
        /// Adds a show function to the design document.
        /// </summary>
        /// <param name="Name">The name of the show function.</param>
        /// <param name="Function">The show function code.</param>
        public void AddShow(string Name, string Function)
        {
            EnsureExists(Shows, Name, Function);
            Shows[Name] = Function;
        }

        /// <summary>
        /// Adds a filter function to the design document.
        /// </summary>
        /// <param name="Name">The name of the filter function.</param>
        /// <param name="Function">The filter function code.</param>
        public void AddFilter(string Name, string Function)
        {
            EnsureExists(Filters, Name, Function);
            Filters[Name] = Function;
        }

        /// <summary>
        /// Adds a list function to the design document.
        /// </summary>
        /// <param name="Name">The name of the list function.</param>
        /// <param name="Function">The list function code.</param>
        public void AddList(string Name, string Function)
        {
            EnsureExists(Lists, Name, Function);
            Lists[Name] = Function;
        }

        /// <summary>
        /// Adds an update function to the design document.
        /// </summary>
        /// <param name="Name">The name of the update function.</param>
        /// <param name="Function">The update function code.</param>
        public void AddUpdate(string Name, string Function)
        {
            EnsureExists(Updates, Name, Function);
            Updates[Name] = Function;
        }

    }

    public class CouchView
    {
        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("reduce")]
        public string Reduce { get; set; }

    }


}