
using Newtonsoft.Json;

namespace PillowSharp.CouchType
{
    /// <summary>
    /// Information describing the provided partition. It includes document and deleted document counts along with external and active data sizes.
    /// </summary>
    public class PartitionInfo
    {
        [JsonProperty("db_name")]
        public string DbName { get; set; }

        [JsonProperty("sizes")]
        public PartitionInfoSize Sizes { get; set; }

        [JsonProperty("partition")]
        public string Partition { get; set; }

        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        [JsonProperty("doc_del_count")]
        public int DocDelCount { get; set; }
    }

    public class PartitionInfoSize
    {
        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("external")]
        public int External { get; set; }
    }
}