using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class UserInputStep : IStep
{
    [JsonPropertyName("content")]
    public List<Content> Content { get; set; }
}
