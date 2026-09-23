using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class Function : ITool
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FunctionParameters? Parameters { get; set; }
}

public class FunctionParameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";
    [JsonPropertyName("properties")]
    public Dictionary<string, FunctionParameter> Properties { get; set; } = [];
    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = [];
}

public class FunctionParameter
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
}
