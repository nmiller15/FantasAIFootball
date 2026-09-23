using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

[JsonConverter(typeof(StepJsonConverter))]
public interface IStep
{
}
