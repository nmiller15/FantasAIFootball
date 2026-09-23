using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Search;

public class QueryResult
{
    [JsonPropertyName("query")]
    public string Query { get; set; }
    [JsonPropertyName("follow_up_questions")]
    public List<string>? FollowUpQuestions { get; set; }
    [JsonPropertyName("answer")]
    public string? Answer { get; set; }
    [JsonPropertyName("results")]
    public List<SearchResult> Results { get; set; }
}

public class SearchResult
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("score")]
    public decimal Score { get; set; }
}
