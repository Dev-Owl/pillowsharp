using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PillowSharp.CouchType
{
    /// <summary>
    /// Gets information about the specified database.
    /// </summary>
    public class CouchDbInfo
    {
        public const string DbPropsPartitioned = "partitioned";

        /// <summary>
        /// Always "0". (Returned for legacy reasons.)
        /// </summary>
        [Obsolete("From CouchDB docs: Always \"0\". (Returned for legacy reasons.)")]
        [JsonProperty("instance_start_time")]
        public string InstanceStartTime { get; set; }

        /// <summary>
        /// The name of the database
        /// </summary>
        [JsonProperty("db_name")]
        public string DbName { get; set; }

        /// <summary>
        /// An opaque string that describes the purge state of the database. Do not rely on this string for counting the number of purge operations.
        /// </summary>
        [JsonProperty("purge_seq")]
        public string PurgeSeq { get; set; }

        /// <summary>
        /// An opaque string that describes the state of the database. Do not rely on this string for counting the number of updates.
        /// </summary>
        [JsonProperty("update_seq")]
        public string UpdateSeq { get; set; }

        /// <summary>
        ///  Information about the sizes of the live database in bytes
        /// </summary>
        [JsonProperty("sizes")]
        public CouchDbInfoSize Sizes { get; set; }

        /// <summary>
        ///  Can be null, shows different props of the database. For now can only contain partitioned as boolean. Please see the DbProps* const(s) in CoucDbInfo
        /// </summary>
        [JsonProperty("props")]
        public Dictionary<string, object> Props { get; set; }

        /// <summary>
        /// Number of deleted documents
        /// </summary>
        [JsonProperty("doc_del_count")]
        public int DocDelCount { get; set; }

        /// <summary>
        /// A count of the documents in the specified database.
        /// </summary>
        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        /// <summary>
        /// The version of the physical format used for the data when it is stored on disk.
        /// </summary>
        [JsonProperty("disk_format_version")]
        public int DiskFormatVersion { get; set; }

        /// <summary>
        /// Set to true if the database compaction routine is operating on this database.
        /// </summary>
        [JsonProperty("compact_running")]
        public bool CompactRunning { get; set; }

        [JsonProperty("cluster")]
        public CouchDBInfoCluster Cluster { get; set; }
    }

    public class CouchDBInfoCluster
    {
        /// <summary>
        ///  Shards. The number of range partitions.
        /// </summary>
        [JsonProperty("q")]
        public int Q { get; set; }

        /// <summary>
        /// Read quorum. The number of consistent copies of a document that need to be read before a successful reply.
        /// </summary>
        [JsonProperty("r")]
        public int R { get; set; }

        /// <summary>
        /// Replicas. The number of copies of every document.
        /// </summary>
        [JsonProperty("n")]
        public int N { get; set; }

        /// <summary>
        /// Write quorum. The number of copies of a document that need to be written before a successful reply.
        /// </summary>
        [JsonProperty("w")]
        public int W { get; set; }

    }


    public class CouchDbInfoSize
    {
        /// <summary>
        /// The size of the database file on disk in bytes. Views indexes are not included in the calculation.
        /// </summary>
        [JsonProperty("file")]
        public int File { get; set; }

        /// <summary>
        /// The uncompressed size of database contents in bytes.
        /// </summary>
        [JsonProperty("external")]
        public int External { get; set; }

        /// <summary>
        ///  The size of live data inside the database, in bytes.
        /// </summary>
        [JsonProperty("active")]
        public int Active { get; set; }

    }
}