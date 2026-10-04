using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

/// <summary>
/// League-level settings. Sleeper returns these as loosely typed values, so every
/// field is nullable and typed to match the raw payload. A type mismatch here
/// aborts deserialization of the entire league object.
/// </summary>
public class LeagueSettings
{
    /// <summary>0 = free agent (rolling), 1 = waiver priority (weekly).</summary>
    [JsonPropertyName("waiver_type")]
    public int? WaiverType { get; set; }

    /// <summary>Day the weekly waiver period clears, where 0 = Sunday.</summary>
    [JsonPropertyName("waiver_day_of_week")]
    public int? WaiverDayOfWeek { get; set; }

    /// <summary>FAAB budget; greater than zero means FAAB is in use.</summary>
    [JsonPropertyName("waiver_budget")]
    public int? WaiverBudget { get; set; }

    /// <summary>Days the waiver period stays open after the final game of the week.</summary>
    [JsonPropertyName("waiver_clear_days")]
    public int? WaiverClearDays { get; set; }

    [JsonPropertyName("waiver_bid_min")]
    public int? WaiverBidMin { get; set; }

    /// <summary>Non-zero when waivers process daily rather than weekly.</summary>
    [JsonPropertyName("daily_waivers")]
    public int? DailyWaivers { get; set; }

    [JsonPropertyName("daily_waivers_hour")]
    public int? DailyWaiversHour { get; set; }

    [JsonPropertyName("faab_suggestions")]
    public int? FaabSuggestions { get; set; }

    /// <summary>Week after which trading is closed. Null when trades never close.</summary>
    [JsonPropertyName("trade_deadline")]
    public int? TradeDeadline { get; set; }

    [JsonPropertyName("disable_trades")]
    public int? DisableTrades { get; set; }

    [JsonPropertyName("playoff_week_start")]
    public int? PlayoffWeekStart { get; set; }

    [JsonPropertyName("playoff_teams")]
    public int? PlayoffTeams { get; set; }

    [JsonPropertyName("playoff_round_type")]
    public int? PlayoffRoundType { get; set; }

    [JsonPropertyName("best_ball")]
    public int? BestBall { get; set; }

    [JsonPropertyName("num_teams")]
    public int? NumTeams { get; set; }

    [JsonPropertyName("start_week")]
    public int? StartWeek { get; set; }
}