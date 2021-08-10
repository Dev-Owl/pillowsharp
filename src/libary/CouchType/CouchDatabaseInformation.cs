using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PillowSharp.CouchType
{
    public class CouchDatabaseInformation
    {
        [JsonProperty("db_name")]
        public String DBName { get; set; }

        [JsonProperty("purge_seq")]
        public String PurgeSequence { get; set; }

        [JsonProperty("update_seq")]
        public String UpdateSequence { get; set; }

        [JsonProperty("sizes")]
        public Dictionary<String, long> Sizes { get; set; }

        [JsonIgnore]
        public long SizeFileSystemInByte { get { return Sizes["file"]; } }

        [JsonIgnore]
        public long SizeExternalInByte { get { return Sizes["external"]; } }

        [JsonIgnore]
        public long SizeActiveInByte { get { return Sizes["active"]; } }

        [JsonProperty("props")]
        public Dictionary<String,String> Properties { get; set; }

        [JsonProperty("doc_del_count")]
        public long DocumentDeletionCount { get; set; }

        [JsonProperty("doc_count")]
        public long DocumentCount { get; set; }

        [JsonProperty("disk_format_version")]
        public long DiskFormatVersion { get; set; }

        [JsonProperty("compact_running")]
        public bool IsCompactRunning { get; set; }

        [JsonProperty("cluster")]
        public Dictionary<String, long> Cluster { get; set; }

        [JsonProperty("instance_start_time")]
        public String InstanceStartTime { get; set; }
    }
}
