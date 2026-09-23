using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
}
