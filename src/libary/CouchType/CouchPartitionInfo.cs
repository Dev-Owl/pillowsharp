
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace PillowSharp.CouchType
{
    /// <summary>
    /// Information describing the provided partition. It includes document and deleted document counts along with external and active data sizes.
    /// </summary>
    public class PartitionInfo
    {
        [JsonProperty("db_name")]
        [JsonPropertyName("db_name")]
        public string DbName { get; set; }

        [JsonProperty("sizes")]
        [JsonPropertyName("sizes")]
        public PartitionInfoSize Sizes { get; set; }

        [JsonProperty("partition")]
        [JsonPropertyName("partition")]
        public string Partition { get; set; }

        [JsonProperty("doc_count")]
        [JsonPropertyName("doc_count")]
        public int DocCount { get; set; }

        [JsonProperty("doc_del_count")]
        [JsonPropertyName("doc_del_count")]
        public int DocDelCount { get; set; }
    }

    public class PartitionInfoSize
    {
        [JsonProperty("active")]
        [JsonPropertyName("active")]
        public int Active { get; set; }

        [JsonProperty("external")]
        [JsonPropertyName("external")]
        public int External { get; set; }
    }
}