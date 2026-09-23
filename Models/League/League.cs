using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class League
{
    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; }
    [JsonPropertyName("season")]
    public string Season { get; set; }
    [JsonPropertyName("season_type")]
    public string SeasonType { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("total_rosters")]
    public int TotalRosters { get; set; }
    [JsonPropertyName("scoring_settings")]
    public ScoringSettings ScoringSettings { get; set; }
    [JsonPropertyName("roster_positions")]
    public List<string> RosterPositions { get; set; }
}
