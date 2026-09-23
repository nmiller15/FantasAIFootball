using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class Matchup
{
    [JsonPropertyName("starters")]
    public List<string> Starters { get; set; }
    [JsonPropertyName("roster_id")]
    public int RosterId { get; set; }
    [JsonPropertyName("players")]
    public List<string> Players { get; set; }
    [JsonPropertyName("matchup_id")]
    public int MatchupId { get; set; }
    [JsonPropertyName("points")]
    public double Points { get; set; }
}
