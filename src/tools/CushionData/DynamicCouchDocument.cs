using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PillowSharp.CouchType;

public class DynamicCouchDocument : CouchDocument
{
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalProperties { get; set; } = new Dictionary<string, object?>();

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}