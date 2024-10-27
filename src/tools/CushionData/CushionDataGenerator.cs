using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using PillowSharp.BaseObject;
using PillowSharp.Client;
using PillowSharp.Middleware.Default;

public class CushionDataGenerator
{
    private readonly PillowClient _client;
    private readonly string _databaseName;

    public bool DryMode { get; set; }

    public Action<DynamicCouchDocument>? DocumentCallback { get; set; }

    private string? _UserContent;
    public string? UserContent
    {
        get
        {
            return _UserContent;
        }
        set
        {
            _UserContent = value;
            _userContentDictionary = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            _userContentDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(value)!;
        }
    }

    private Dictionary<string, object> _userContentDictionary = new Dictionary<string, object>();

    public CushionDataGenerator(string databaseName, string couchDbUrl, string username, string password)
    {
        _databaseName = databaseName;
        _client = new PillowClient(new BasicCouchDBServer(couchDbUrl, new CouchLoginData(username, password), ELoginTypes.BasicAuth));
        _client.JSONHelper = new CushionDataJsonMiddleware();
    }

    public async Task GenerateDocumentsAsync(string templateFileContent, int numberOfDocuments, int batchSize = 1)
    {
        // Create the database if it does not exist
        if (DryMode == false && await _client.DbExistsAsync(_databaseName) == false)
        {
            await _client.CreateNewDatabaseAsync(_databaseName);
        }

        var template = templateFileContent;
        var documentTemplate = JsonSerializer.Deserialize<Dictionary<string, object>>(template)!;

        for (int i = 0; i < numberOfDocuments; i += batchSize)
        {
            var batch = new List<DynamicCouchDocument>();
            for (int j = 0; j < batchSize && i + j < numberOfDocuments; j++)
            {
                var nextDocument = GenerateDocument(documentTemplate);
                DocumentCallback?.Invoke(nextDocument);
                batch.Add(nextDocument);
            }
            if (DryMode == false)
                await _client.UpdateDocumentsAsync(batch, _databaseName);

        }
    }


    private DynamicCouchDocument GenerateDocument(Dictionary<string, object> template)
    {
        var newDocument = new DynamicCouchDocument();

        foreach (var field in template)
        {
            // Check if the filed is the _id field and directly set the Id Property
            if (field.Key == "_id")
            {
                newDocument.ID = GenerateFieldData(field.Value)?.ToString();
                continue;
            }
            newDocument.AdditionalProperties.Add(field.Key, GenerateFieldData(field.Value));
        }

        return newDocument;
    }

    private object? GenerateFieldData(object fieldTemplate)
    {
        // Implement data generation logic based on the field template
        // For example, if the field template specifies a type "string", generate a random string
        // This is a simple example, you can expand it to handle different types and constraints
        if (fieldTemplate is JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    var fieldType = jsonElement.GetString()?.Split(":", StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    return fieldType switch
                    {
                        "string" => GenerateRandomString(jsonElement),
                        "int" => GenerateRandomInt(jsonElement),
                        "decimal" => GenerateRandomDecimal(jsonElement),
                        "datetime" => GenerateRandomDateTime(jsonElement),
                        "array" => GenerateArrayField(jsonElement),
                        "uuid" => Guid.NewGuid().ToString(),
                        "partion" => GeneratePartionKey(jsonElement),
                        "bool" => new Random().Next(0, 2) == 1,
                        _ => fieldType ?? null
                    };
                case JsonValueKind.Object:
                    var subObjectTemplate = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText());
                    return GenerateSubObject(subObjectTemplate);
                case JsonValueKind.Array:
                    var arrayTemplate = JsonSerializer.Deserialize<List<object>>(jsonElement.GetRawText());
                    return GenerateArrayFieldFromTemplate(arrayTemplate?.FirstOrDefault()?.ToString() ?? "");
            }
        }

        return null;
    }

    private string GeneratePartionKey(JsonElement jsonElement)
    {
        if (jsonElement.GetString()?.Contains(":list(") == true)
        {
            var listMatch = Regex.Match(jsonElement.GetString()!, @":list\(([^)]+)\)");
            if (listMatch.Success)
            {
                var listName = listMatch.Groups[1].Value;
                // check if the list exists in the user content
                if (_userContentDictionary.ContainsKey(listName))
                {
                    var list = ((JsonElement)_userContentDictionary[listName]).Deserialize<List<String>>();
                    if (list != null)
                    {
                        return $"{list[new Random().Next(0, list.Count)]}:{Guid.NewGuid()}";
                    }
                }
            }
        }
        return $"PillowSharp:{Guid.NewGuid()}";
    }

    private string GenerateRandomString(JsonElement jsonElement)
    {
        int minLength = 10;
        int maxLength = 10;
        if (jsonElement.GetString()?.Contains(":range(") == true)
        {
            var rangeMatch = Regex.Match(jsonElement.GetString()!, @":range\((\d+)(?:,(\d+))?\)");
            if (rangeMatch.Success)
            {
                minLength = int.Parse(rangeMatch.Groups[1].Value);
                if (rangeMatch.Groups[2].Success)
                {
                    maxLength = int.Parse(rangeMatch.Groups[2].Value);
                }
            }
        }


        // Ensure minlength is not greater than maxLength
        minLength = Math.Min(minLength, maxLength);
        if (jsonElement.GetString()?.Contains(":list(") == true)
        {
            var listMatch = Regex.Match(jsonElement.GetString()!, @":list\(([^)]+)\)");
            if (listMatch.Success)
            {
                var listName = listMatch.Groups[1].Value;
                // check if the list exists in the user content
                if (_userContentDictionary.ContainsKey(listName))
                {
                    var list = ((JsonElement)_userContentDictionary[listName]).Deserialize<List<String>>();
                    if (list != null)
                    {
                        return list[new Random().Next(0, list.Count)];
                    }
                }
            }
        }

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, random.Next(minLength, maxLength + 1))
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public object? GenerateArrayFieldFromTemplate(string arrayTemplate)
    {
        if (string.IsNullOrEmpty(arrayTemplate))
        {
            return null;
        }
        var minLength = 1;
        var maxLength = 1;
        var rangeMatch = Regex.Match(arrayTemplate, @":range\((\d+)(?:,(\d+))?\)");
        if (rangeMatch.Success)
        {
            minLength = int.Parse(rangeMatch.Groups[1].Value);
            if (rangeMatch.Groups[2].Success)
            {
                maxLength = int.Parse(rangeMatch.Groups[2].Value);
            }
            else
            {
                maxLength = minLength;
            }
            arrayTemplate = arrayTemplate.Substring(0, rangeMatch.Index);
        }

        var arrayTemplateObject = JsonSerializer.Deserialize<JsonElement>(arrayTemplate);
        var array = new List<object>();
        var random = new Random();
        var length = random.Next(minLength, maxLength + 1);

        for (int i = 0; i < length; i++)
        {
            array.Add(GenerateFieldData(arrayTemplateObject));
        }
        return array;
    }

    private object? GenerateArrayField(JsonElement jsonElement)
    {
        var array = new List<object>();
        var fieldType = jsonElement.GetString()!;
        var minLength = 1;
        var maxLength = 1;
        var lengthMatch = Regex.Match(fieldType, @"\:range\((\d+)(?:,(\d+))?\)");
        var arrayValues = new List<object>();
        if (lengthMatch.Success)
        {
            minLength = int.Parse(lengthMatch.Groups[1].Value);
            if (lengthMatch.Groups[2].Success)
            {
                maxLength = int.Parse(lengthMatch.Groups[2].Value);
            }
            else
            {
                maxLength = minLength;
            }
        }
        // Ensure minlength is not greater than maxLength
        minLength = Math.Min(minLength, maxLength);

        if (Regex.IsMatch(fieldType, @":value\(([^)]+)\)"))
        {
            // Use regex to get all matches of text or numbers between commas within the :value(CONTENT)
            var regex = new Regex(@":value\(([^)]+)\)");
            var match = regex.Match(fieldType);
            if (match.Success)
            {
                var valuesStrings = match.Groups[1].Value;
                var valueRegex = new Regex(@"([^,]+)");
                var valueMatches = valueRegex.Matches(valuesStrings);
                foreach (Match valueMatch in valueMatches)
                {
                    if (int.TryParse(valueMatch.Value.Trim(), out int intValue))
                    {
                        arrayValues.Add(intValue);
                    }
                    else if (decimal.TryParse(valueMatch.Value.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal decimalValue))
                    {
                        arrayValues.Add(decimalValue);
                    }
                    else if (DateTime.TryParse(valueMatch.Value.Trim(), out DateTime dateTimeValue))
                    {
                        arrayValues.Add(dateTimeValue);
                    }
                    else
                        arrayValues.Add(valueMatch.Value.Trim());
                }
            }

        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                arrayValues.Add(Guid.NewGuid().ToString());
            }
        }
        // For the given length create a string contain a random value from the arrayValues list
        var length = new Random().Next(minLength, maxLength);
        for (int i = 0; i < length; i++)
        {
            array.Add(arrayValues[new Random().Next(0, arrayValues.Count)]);
        }
        return array;
    }

    private int GenerateRandomInt(JsonElement jsonElement)
    {
        var min = 0;
        var max = int.MaxValue;
        if (jsonElement.ValueKind == JsonValueKind.String && jsonElement.GetString()?.Contains(":") == true)
        {
            var range = jsonElement.GetString()?.Split(":", StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            if (range != null)
            {
                var rangeValues = range.Split(",", StringSplitOptions.RemoveEmptyEntries);
                // Remove any chars from each string in rangeValues that are not numbers
                rangeValues = rangeValues.Select(value => new string(value.Where(char.IsDigit).ToArray())).ToArray();

                // Allow to only passing the lower bound
                min = int.Parse(rangeValues[0].Trim());
                if (rangeValues.Length == 1)
                {
                    max = int.MaxValue;
                }
                else
                {
                    max = int.Parse(rangeValues[1].Trim());
                }
            }
        }
        return new Random().Next(min, max);
    }

    private decimal GenerateRandomDecimal(JsonElement jsonElement)
    {
        var min = 0m;
        var max = 100m;
        if (jsonElement.ValueKind == JsonValueKind.String && jsonElement.GetString()?.Contains(":") == true)
        {
            var range = jsonElement.GetString()?.Split(":", StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            if (range != null)
            {
                var rangeValues = range.Split(",", StringSplitOptions.RemoveEmptyEntries);
                // Remove any chars from each string in rangeValues that are not numbers or decimal points
                rangeValues = rangeValues.Select(value => new string(value.Where(c => char.IsDigit(c) || c == '.').ToArray())).ToArray();

                // Allow to only passing the lower bound
                min = decimal.Parse(rangeValues[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                if (rangeValues.Length == 1)
                {
                    max = 100m;
                }
                else
                {
                    max = decimal.Parse(rangeValues[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }
        var random = new Random();
        return Convert.ToDecimal(random.NextDouble() * (double)(max - min) + (double)min);
    }

    private DateTime GenerateRandomDateTime(JsonElement jsonElement)
    {
        var random = new Random();
        var start = new DateTime(2000, 1, 1);
        var range = (DateTime.Today - start).Days;
        if (jsonElement.GetString()?.StartsWith("datetime:range(") == true)
        {
            var rangeConfig = ParseDateTimeRange(jsonElement.GetString()!);
            start = rangeConfig.start;
            range = (rangeConfig.end - start).Days;
        }
        return start.AddDays(random.Next(range)).AddSeconds(random.Next(86400));
    }

    private (DateTime start, DateTime end) ParseDateTimeRange(string fieldType)
    {
        var rangeValues = fieldType.Substring("datetime:range(".Length).TrimEnd(')').Split(',');
        DateTime start = DateTime.Parse(rangeValues[0]);
        DateTime end = DateTime.Parse(rangeValues[1]);
        return (start, end);
    }

    private Dictionary<string, object?> GenerateSubObject(Dictionary<string, object> subObjectTemplate)
    {
        var subObject = new Dictionary<string, object?>();

        foreach (var field in subObjectTemplate)
        {
            subObject[field.Key] = GenerateFieldData(field.Value);
        }

        return subObject;
    }
}