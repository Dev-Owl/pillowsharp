using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PillowSharp.Middleware;
using PillowSharp.BaseObject;

namespace PillowSharp.CouchType
{
    public class MangoQuery
    {
        [JsonProperty(PropertyName = "selector")]
        public MangoSelector Selector { get; set; }

        [JsonProperty(PropertyName = "sort")]
        public List<MangoSort> Sort { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public List<string> FilteringFields { get; set; }

        [JsonProperty(PropertyName = "limit")]
        public int Limit { get; set; } = 25;

        [JsonProperty(PropertyName = "skip")]
        public int Skip { get; set; }

        [JsonProperty(PropertyName = "execution_stats")]
        public bool IncludeExecutionStats { get; set; }

        [JsonProperty(PropertyName = "bookmark")]
        public string Bookmark { get; set; }
        [JsonProperty(PropertyName = "use_index")]
        public List<string> UseIndex { get; set; }
        public void Validate()
        {
            if((Selector?.Operations?.Count ?? 0) == 0)
                throw new PillowException("Queries must have at least one selector");
        }

    }

    public class MangoSort
    {
        public string Field { get; set; }

        public bool OrderAsc { get; set; } = true;
    }

    public class MangoSortConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MangoSort);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Too lazy");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var result = value as MangoSort;
            var currentObject = new JObject();
            currentObject.Add(result.Field, result.OrderAsc ? "asc" : "desc");
            currentObject.WriteTo(writer);
        }
    }

    public class MangoSelector
    {
        [JsonIgnore]
        public List<MangoSelectorOperator> Operations { get; set; }
    }

    public class MangoSelectorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MangoSelector);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Too lazy");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var selectors = value as MangoSelector;
            writer.WriteStartObject();
            foreach (var operation in selectors.Operations)
            {
                var prop = GenerateJson(operation);
                var dataProp = prop.Properties().First();
                writer.WritePropertyName(dataProp.Name);
                if (dataProp.Value is JArray)
                {
                    dataProp.Value.WriteTo(writer);

                }
                else
                {
                    writer.WriteValue(dataProp.Value);
                }

            }
            writer.WriteEndObject();
        }

        private JObject GenerateJson(MangoSelectorOperator selector)
        {
            var currentObject = new JObject();
            if (selector.SimpleOperatorValue != null)
                currentObject.Add(selector.OperatorKey, JToken.FromObject(selector.SimpleOperatorValue));
            else if (selector.OperatorValues.Count == 1)
            {
                currentObject.Add(selector.OperatorKey, GenerateJson(selector.OperatorValues.First()));
            }
            else
            {
                var jArray = new JArray();
                foreach (var operators in selector.OperatorValues)
                {
                    jArray.Add(GenerateJson(operators));
                }
                currentObject.Add(selector.OperatorKey, jArray);
            }
            return currentObject;
        }
    }

    public class MangoSelectorOperator
    {
        public string OperatorKey { get; set; }

        public object SimpleOperatorValue { get; set; }

        public List<MangoSelectorOperator> OperatorValues { get; set; }

        public MangoSelectorOperator(string Operator)
        {
            OperatorKey = Operator;
        }
    }

}