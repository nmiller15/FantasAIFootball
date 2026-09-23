using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Search;

public class ExtractResponse
{
    [JsonPropertyName("results")]
    public List<ExtractResult> Results { get; set; }
    [JsonPropertyName("response_time")]
    public decimal ResponseTime { get; set; }

}

public class ExtractResult
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("raw_content")]
    public string RawContent { get; set; }
}
