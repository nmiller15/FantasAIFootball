using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class ThoughtStep : IStep
{
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
    [JsonPropertyName("summary")]
    public List<Content>? Summary { get; set; }
}
