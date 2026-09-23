using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Search;

public class ExtractRequestBody
{
    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; }
}
