using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class ModelOutputStep : IStep
{
    [JsonPropertyName("content")]
    public List<Content> Content { get; set; }
}
