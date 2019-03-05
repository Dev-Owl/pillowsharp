using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PillowSharp.Middleware;
using PillowSharp.BaseObject;

namespace PillowSharp.CouchType
{
    public class MangoQueryResult<T>
    {
        [JsonProperty(PropertyName = "docs")]
        public List<T> Docs { get; set; }

        [JsonProperty(PropertyName="bookmark")]
        public string Bookmark { get; set; }
        
        [JsonProperty(PropertyName="warning")]
        public string Warning { get; set; }

        [JsonProperty(PropertyName="execution_stats")]
        public ExecutionStats ExecutionStats { get; set; }
    }

    public class ExecutionStats
    {

        [JsonProperty(PropertyName= "total_keys_examined")]
        public int TotalKeysExamined { get; set; }
        
        [JsonProperty(PropertyName= "total_docs_examined")]
        public int TotalDocsExamined { get; set; }

        [JsonProperty(PropertyName="total_quorum_docs_examined")]
        public int TotalQuorumDocsExamined { get; set; }

        [JsonProperty("results_returned")]
        public int ResultsReturned { get; set; }

        [JsonProperty("execution_time_ms")]
        public decimal ExecutionTimeMs { get; set; }


    }
}