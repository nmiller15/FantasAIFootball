using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Memories;

public class Recommendation
{
    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; }
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    [JsonPropertyName("player_name")]
    public string PlayerName { get; set; }
    [JsonPropertyName("direction")]
    public string Direction { get; set; }
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
