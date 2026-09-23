using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class Roster
{
    [JsonPropertyName("starters")]
    public List<string> Starters { get; set; }
    [JsonPropertyName("settings")]
    public RosterSettings Settings { get; set; }
    [JsonPropertyName("roster_id")]
    public int RosterId { get; set; }
    [JsonPropertyName("reserve")]
    public List<string> Reserve { get; set; }
    [JsonPropertyName("players")]
    public List<string> Players { get; set; }
    [JsonPropertyName("owner_id")]
    public string OwnerId { get; set; }
    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; }
}
