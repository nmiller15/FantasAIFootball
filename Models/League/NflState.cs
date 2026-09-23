using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class NflState
{
    [JsonPropertyName("week")]
    public int Week { get; set; }
    [JsonPropertyName("season_type")]
    public string SeasonType { get; set; }
    [JsonPropertyName("season_start_date")]
    public DateTime SeasonStartDate { get; set; }
    [JsonPropertyName("season")]
    public string Season { get; set; }
    [JsonPropertyName("previous_season")]
    public string PreviousSeason { get; set; }
    [JsonPropertyName("leg")]
    public int Leg { get; set; }
    [JsonPropertyName("league_season")]
    public string LeagueSeason { get; set; }
    [JsonPropertyName("league_create_season")]
    public string LeagueCreateSeason { get; set; }
    [JsonPropertyName("display_week")]
    public int DisplayWeek { get; set; }
}
