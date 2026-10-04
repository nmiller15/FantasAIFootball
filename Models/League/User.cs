using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("is_owner")]
    public bool? IsOwner { get; set; }

    /// <summary>Sleeper nests the team nickname inside metadata.</summary>
    [JsonPropertyName("metadata")]
    public UserMetadata? Metadata { get; set; }
}

public class UserMetadata
{
    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}