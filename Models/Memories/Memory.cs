using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Memories;

public class Memory
{
    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; }
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
