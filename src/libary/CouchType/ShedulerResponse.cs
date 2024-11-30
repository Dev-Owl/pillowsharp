using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace PillowSharp.CouchType
{
    public class SchedulerResponse
    {
        [JsonProperty("jobs")]
        [JsonPropertyName("jobs")]
        public List<SchedulerJob> Jobs { get; set; }

        [JsonProperty("offset")]
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonProperty("total_rows")]
        [JsonPropertyName("total_rows")]
        public int TotalRows { get; set; }
    }

    public class SchedulerJob
    {
        [JsonProperty("database")]
        [JsonPropertyName("database")]
        public string Database { get; set; }

        [JsonProperty("doc_id")]
        [JsonPropertyName("doc_id")]
        public string DocId { get; set; }

        [JsonProperty("history")]
        [JsonPropertyName("history")]
        public List<HistoryItem> History { get; set; }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("info")]
        [JsonPropertyName("info")]
        public Info Info { get; set; }

        [JsonProperty("node")]
        [JsonPropertyName("node")]
        public string Node { get; set; }

        [JsonProperty("pid")]
        [JsonPropertyName("pid")]
        public string Pid { get; set; }

        [JsonProperty("source")]
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonProperty("start_time")]
        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("target")]
        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonProperty("user")]
        [JsonPropertyName("user")]
        public string User { get; set; }
    }

    public class HistoryItem
    {
        [JsonProperty("timestamp")]
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Info
    {
        [JsonProperty("changes_pending")]
        [JsonPropertyName("changes_pending")]
        public int? ChangesPending { get; set; }

        [JsonProperty("checkpointed_source_seq")]
        [JsonPropertyName("checkpointed_source_seq")]
        public string CheckpointedSourceSeq { get; set; }

        [JsonProperty("doc_write_failures")]
        [JsonPropertyName("doc_write_failures")]
        public int DocWriteFailures { get; set; }

        [JsonProperty("docs_read")]
        [JsonPropertyName("docs_read")]
        public int DocsRead { get; set; }

        [JsonProperty("docs_written")]
        [JsonPropertyName("docs_written")]
        public int DocsWritten { get; set; }

        [JsonProperty("bulk_get_attempts")]
        [JsonPropertyName("bulk_get_attempts")]
        public int BulkGetAttempts { get; set; }

        [JsonProperty("bulk_get_docs")]
        [JsonPropertyName("bulk_get_docs")]
        public int BulkGetDocs { get; set; }

        [JsonProperty("missing_revisions_found")]
        [JsonPropertyName("missing_revisions_found")]
        public int MissingRevisionsFound { get; set; }

        [JsonProperty("revisions_checked")]
        [JsonPropertyName("revisions_checked")]
        public int RevisionsChecked { get; set; }

        [JsonProperty("source_seq")]
        [JsonPropertyName("source_seq")]
        public string SourceSeq { get; set; }

        [JsonProperty("through_seq")]
        [JsonPropertyName("through_seq")]
        public string ThroughSeq { get; set; }
    }
}