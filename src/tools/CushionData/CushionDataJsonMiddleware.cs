using System.Text.Json;
using System.Text.Json.Serialization;
using pillowsharp.Middleware;
using PillowSharp.Middleware;

public class CushionDataJsonMiddleware : IJSONHelper
{
    public string ToJSON(object Data)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(Data, options);
    }

    public T FromJSON<T>(string JSON) where T : new()
    {
        return JsonSerializer.Deserialize<T>(JSON);
    }

    public T FromJSON<T>(RestResponse Response) where T : new()
    {
        return JsonSerializer.Deserialize<T>(Response.Content);
    }
}