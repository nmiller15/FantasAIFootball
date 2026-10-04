using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

/// <summary>
/// A league-wide transaction (trade, waiver, or free agent move) as returned by
/// Sleeper's league transactions endpoint. Only transactions the requesting user
/// is involved in are guaranteed to be present.
/// </summary>
public class Transaction
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("status_updated")]
    public long? StatusUpdated { get; set; }

    /// <summary>In football this is the week the transaction belongs to.</summary>
    [JsonPropertyName("leg")]
    public int? Leg { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }

    [JsonPropertyName("league_id")]
    public string? LeagueId { get; set; }

    [JsonPropertyName("roster_id")]
    public int? RosterId { get; set; }

    [JsonPropertyName("roster_ids")]
    public List<int>? RosterIds { get; set; }

    [JsonPropertyName("counter_roster_id")]
    public int? CounterRosterId { get; set; }

    [JsonPropertyName("other_player_id")]
    public string? OtherPlayerId { get; set; }

    [JsonPropertyName("creator")]
    public string? Creator { get; set; }

    [JsonPropertyName("consenter_ids")]
    public List<int>? ConsenterIds { get; set; }

    /// <summary>Maps player id to the roster id the player moved onto.</summary>
    [JsonPropertyName("adds")]
    public Dictionary<string, int>? Adds { get; set; }

    /// <summary>Maps player id to the roster id the player moved off of.</summary>
    [JsonPropertyName("drops")]
    public Dictionary<string, int>? Drops { get; set; }

    [JsonPropertyName("trade_details")]
    public List<TradeDetail>? TradeDetails { get; set; }

    [JsonPropertyName("draft_picks")]
    public List<DraftPickTrade>? DraftPicks { get; set; }

    [JsonPropertyName("waiver_budget")]
    public List<WaiverBudgetEntry>? WaiverBudget { get; set; }

    [JsonPropertyName("settings")]
    public TransactionSettings? Settings { get; set; }

    [JsonPropertyName("metadata")]
    public TransactionMetadata? Metadata { get; set; }
}

public class TradeDetail
{
    [JsonPropertyName("roster_id")]
    public int? RosterId { get; set; }

    [JsonPropertyName("from")]
    public int? From { get; set; }

    [JsonPropertyName("to")]
    public int? To { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }

    [JsonPropertyName("draft_picks")]
    public List<DraftPickTrade>? DraftPicks { get; set; }
}

/// <summary>A draft pick that changed hands as part of a trade.</summary>
public class DraftPickTrade
{
    [JsonPropertyName("season")]
    public string? Season { get; set; }

    [JsonPropertyName("round")]
    public int? Round { get; set; }

    [JsonPropertyName("roster_id")]
    public int? RosterId { get; set; }

    [JsonPropertyName("previous_owner_id")]
    public int? PreviousOwnerId { get; set; }

    [JsonPropertyName("owner_id")]
    public int? OwnerId { get; set; }
}

public class WaiverBudgetEntry
{
    [JsonPropertyName("sender")]
    public int? Sender { get; set; }

    [JsonPropertyName("receiver")]
    public int? Receiver { get; set; }

    [JsonPropertyName("amount")]
    public int? Amount { get; set; }
}

public class TransactionSettings
{
    [JsonPropertyName("waiver_bid")]
    public int? WaiverBid { get; set; }
}

public class TransactionMetadata
{
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}