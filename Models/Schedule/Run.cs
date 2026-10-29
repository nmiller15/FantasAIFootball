using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Schedule;

public class Run
{
    [JsonPropertyName("key")] public int Key { get; set; }
    [JsonPropertyName("league_id")] public string LeagueId { get; set; }
    [JsonPropertyName("run_date_time")] public DateTime RunDateTime { get; set; }
    [JsonPropertyName("notes")] public string Notes { get; set; }
    [JsonPropertyName("inserted_at")] public DateTime InsertedAt { get; set; }
    [JsonPropertyName("executed_at")] public DateTime? ExecutedAt { get; set; }
    [JsonPropertyName("report_subject")] public string? ReportSubject { get; set; }
    [JsonPropertyName("report_body")] public string? ReportBody { get; set; }
}
