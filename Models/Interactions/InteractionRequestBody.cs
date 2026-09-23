using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class InteractionRequestBody
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("input")]
    public List<IStep> Input { get; set; }
    [JsonPropertyName("system_instruction")]
    public string SystemInstruction { get; set; }
    [JsonPropertyName("tools")]
    public List<Function> Tools { get; set; } = [];
    [JsonPropertyName("store")]
    public bool Store { get; set; } = false;
}










