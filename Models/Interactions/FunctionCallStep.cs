using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class FunctionCallStep : IStep
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("arguments")]
    public JsonElement Arguments { get; set; }
}
