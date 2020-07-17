using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public class PurgeResponse{

    [JsonProperty(PropertyName="purge_seq")]
    public string PurgeSeq { get; set; }

    [JsonProperty(PropertyName="purged")]
    public Dictionary<string, List<String>> Purged { get; set; }

}

public class PurgedDocumentRevisionResponse{
    
    [JsonProperty(PropertyName="purged")]
    public List<String> Purged { get; set; }
}
