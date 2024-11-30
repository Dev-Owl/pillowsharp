using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace PillowSharp.CouchType
{
    public class ReplicationRequest
    {
        [JsonProperty("source")]
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonProperty("create_target")]
        [JsonPropertyName("create_target")]
        public bool? CreateTarget { get; set; }

        [JsonProperty("continuous")]
        [JsonPropertyName("continuous")]
        public bool? Continuous { get; set; }

        [JsonProperty("doc_ids")]
        [JsonPropertyName("doc_ids")]
        public string[] DocIds { get; set; }

        [JsonProperty("filter")]
        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonProperty("proxy")]
        [JsonPropertyName("proxy")]
        public string Proxy { get; set; }
    }

    public class ReplicationResponse
    {
        [JsonProperty("ok")]
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("rev")]
        [JsonPropertyName("rev")]
        public string Rev { get; set; }

        [JsonProperty("_local_id")]
        [JsonPropertyName("_local_id")]
        public string LocalId { get; set; }

        [JsonProperty("no_changes")]
        [JsonPropertyName("no_changes")]
        public bool? NoChanges { get; set; }

        [JsonProperty("session_id")]
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("source_last_seq")]
        [JsonPropertyName("source_last_seq")]
        public string SourceLastSeq { get; set; }

        [JsonProperty("replication_id_version")]
        [JsonPropertyName("replication_id_version")]
        public int? ReplicationIdVersion { get; set; }

        [JsonProperty("history")]
        [JsonPropertyName("history")]
        public ReplicationHistory[] History { get; set; }
    }

    public class ReplicationHistory
    {
        [JsonProperty("doc_write_failures")]
        [JsonPropertyName("doc_write_failures")]
        public int DocWriteFailures { get; set; }

        [JsonProperty("docs_read")]
        [JsonPropertyName("docs_read")]
        public int DocsRead { get; set; }

        [JsonProperty("docs_written")]
        [JsonPropertyName("docs_written")]
        public int DocsWritten { get; set; }

        [JsonProperty("end_last_seq")]
        [JsonPropertyName("end_last_seq")]
        public string EndLastSeq { get; set; }

        [JsonProperty("end_time")]
        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("missing_checked")]
        [JsonPropertyName("missing_checked")]
        public int MissingChecked { get; set; }

        [JsonProperty("missing_found")]
        [JsonPropertyName("missing_found")]
        public int MissingFound { get; set; }

        [JsonProperty("recorded_seq")]
        [JsonPropertyName("recorded_seq")]
        public string RecordedSeq { get; set; }

        [JsonProperty("session_id")]
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("start_last_seq")]
        [JsonPropertyName("start_last_seq")]
        public string StartLastSeq { get; set; }

        [JsonProperty("start_time")]
        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }
    }
}