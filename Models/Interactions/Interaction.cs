using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class Interaction
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("created")]
    public string? Created { get; set; }
    [JsonPropertyName("errors")]
    public List<Error>? Errors { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("steps")]
    public List<IStep>? Steps { get; set; }
}

public class Error
{
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
}
