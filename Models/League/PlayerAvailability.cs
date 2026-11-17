using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class PlayerAvailability
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "unknown";
    [JsonPropertyName("player_name")]
    public string PlayerName { get; set; } = "";
    [JsonPropertyName("player_id")]
    public string? PlayerId { get; set; }
    [JsonPropertyName("roster_id")]
    public int? RosterId { get; set; }
    [JsonPropertyName("owner_name")]
    public string? OwnerName { get; set; }
    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }
    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = "";
}
