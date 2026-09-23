using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

public class FunctionResultStep : IStep
{
    [JsonPropertyName("call_id")]
    public string CallId { get; set; }
    [JsonPropertyName("is_error")]
    public bool IsError { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("result")]
    public List<Content> Result { get; set; } = [];

    public static FunctionResultStep Success(FunctionCallStep call, object output)
    {
        return new FunctionResultStep
        {
            CallId = call.Id,
            IsError = false,
            Name = call.Name,
            Result = new List<Content> { new Content(ToText(output)) }
        };
    }

    // The API expects each result entry to be a content part with a "type" (e.g. text).
    // Callers may pass an already-serialized JSON string or an arbitrary object; either way
    // we surface it as text content.
    private static string ToText(object output)
    {
        return output is string text ? text : JsonSerializer.Serialize(output);
    }

    public static FunctionResultStep Failure(FunctionCallStep call, string errorMessage)
    {
        return new FunctionResultStep
        {
            CallId = call.Id,
            IsError = true,
            Name = call.Name,
            Result = new List<Content> { new Content(JsonSerializer.Serialize(new { error = errorMessage })) }
        };
    }
}
