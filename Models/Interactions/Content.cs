using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class Content
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public Content() {}

    public Content(string text)
    {
        Text = text;
    }
}
