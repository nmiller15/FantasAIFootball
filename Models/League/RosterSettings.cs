using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class RosterSettings
{
    [JsonPropertyName("wins")]
    public int Wins { get; set; }
    [JsonPropertyName("waiver_position")]
    public int WaiverPosition { get; set; }
    [JsonPropertyName("waiver_budget_used")]
    public int WaiverBudgetUsed { get; set; }
    [JsonPropertyName("total_moves")]
    public int TotalMoves { get; set; }
    [JsonPropertyName("ties")]
    public int Ties { get; set; }
    [JsonPropertyName("losses")]
    public int Losses { get; set; }
    [JsonPropertyName("fpts_decimal")]
    public int FptsDecimal { get; set; }
    [JsonPropertyName("fpts_against_decimal")]
    public int FptsAgainstDecimal { get; set; }
    [JsonPropertyName("fpts_against")]
    public int FptsAgainst { get; set; }
    [JsonPropertyName("fpts")]
    public int Fpts { get; set; }
}
