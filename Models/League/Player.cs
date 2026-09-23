using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class Player
{
    [JsonPropertyName("hashtag")]
    public string Hashtag { get; set; }
    [JsonPropertyName("depth_chart_position")]
    public string? DepthChartPosition { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("sport")]
    public string Sport { get; set; }
    [JsonPropertyName("fantasy_positions")]
    public List<string> FantasyPositions { get; set; }
    [JsonPropertyName("number")]
    public int? Number { get; set; }
    [JsonPropertyName("search_last_name")]
    public string SearchLastName { get; set; }
    [JsonPropertyName("injury_start_date")]
    public string? InjuryStartDate { get; set; }
    [JsonPropertyName("weight")]
    public string Weight { get; set; }
    [JsonPropertyName("position")]
    public string Position { get; set; }
    [JsonPropertyName("practice_participation")]
    public string? PracticeParticipation { get; set; }
    [JsonPropertyName("sportradar_id")]
    public string SportradarId { get; set; }
    [JsonPropertyName("team")]
    public string Team { get; set; }
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }
    [JsonPropertyName("college")]
    public string College { get; set; }
    [JsonPropertyName("fantasy_data_id")]
    public int? FantasyDataId { get; set; }
    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; }
    [JsonPropertyName("height")]
    public string Height { get; set; }
    [JsonPropertyName("search_full_name")]
    public string SearchFullName { get; set; }
    [JsonPropertyName("age")]
    public int? Age { get; set; }
    [JsonPropertyName("stats_id")]
    public int? StatsId { get; set; }
    [JsonPropertyName("birth_country")]
    public string BirthCountry { get; set; }
    [JsonPropertyName("espn_id")]
    public int? EspnId { get; set; }
    [JsonPropertyName("search_rank")]
    public int? SearchRank { get; set; }
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }
    [JsonPropertyName("depth_chart_order")]
    public int? DepthChartOrder { get; set; }
    [JsonPropertyName("years_exp")]
    public int? YearsExp { get; set; }
    [JsonPropertyName("rotowire_id")]
    public int? RotowireId { get; set; }
    [JsonPropertyName("rotoworld_id")]
    public int? RotoworldId { get; set; }
    [JsonPropertyName("search_first_name")]
    public string SearchFirstName { get; set; }
    [JsonPropertyName("yahoo_id")]
    public int? YahooId { get; set; }
}
