using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Search;

public class QueryRequestBody
{
    [JsonPropertyName("query")]
    public string Query { get; set; }
    [JsonPropertyName("search_depth")]
    public string SearchDepth { get; set; } = "fast";
    [JsonPropertyName("start_date")]
    public string? StartDate { get; set; }
    [JsonPropertyName("end_date")]
    public string? EndDate { get; set; }
}
