using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class PurgeResponse
{

    [JsonProperty(PropertyName = "purge_seq")]
    [JsonPropertyName("purge_seq")]
    public string PurgeSeq { get; set; }

    [JsonProperty(PropertyName = "purged")]
    [JsonPropertyName("purged")]
    public Dictionary<string, List<String>> Purged { get; set; }

}

public class PurgedDocumentRevisionResponse
{

    [JsonProperty(PropertyName = "purged")]
    [JsonPropertyName("purged")]
    public List<String> Purged { get; set; }
}
