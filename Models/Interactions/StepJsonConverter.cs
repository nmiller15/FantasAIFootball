using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.Interactions;

/// <summary>
/// Custom polymorphic converter for <see cref="IStep"/>.
///
/// The built-in [JsonPolymorphic] support in System.Text.Json requires the type
/// discriminator ("type") to be the FIRST property of each JSON object. The Gemini
/// interactions API returns steps with "type" in a non-first position, which causes
/// deserialization to throw NotSupportedException. This converter reads the "type"
/// property regardless of its position and dispatches to the concrete type.
/// </summary>
public class StepJsonConverter : JsonConverter<IStep>
{
    private const string DiscriminatorName = "type";

    private static readonly Dictionary<string, Type> TypeByDiscriminator = new()
    {
        ["user_input"] = typeof(UserInputStep),
        ["model_output"] = typeof(ModelOutputStep),
        ["function_call"] = typeof(FunctionCallStep),
        ["function_result"] = typeof(FunctionResultStep),
        ["thought"] = typeof(ThoughtStep),
    };

    private static readonly Dictionary<Type, string> DiscriminatorByType =
        TypeByDiscriminator.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public override IStep Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (!root.TryGetProperty(DiscriminatorName, out var discriminatorElement)
            || discriminatorElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException(
                $"Step JSON is missing a valid '{DiscriminatorName}' discriminator property.");
        }

        var discriminator = discriminatorElement.GetString()!;

        if (!TypeByDiscriminator.TryGetValue(discriminator, out var concreteType))
        {
            throw new JsonException(
                $"Unknown step '{DiscriminatorName}' discriminator value: '{discriminator}'.");
        }

        return (IStep)root.Deserialize(concreteType, options)!;
    }

    public override void Write(Utf8JsonWriter writer, IStep value, JsonSerializerOptions options)
    {
        var concreteType = value.GetType();

        if (!DiscriminatorByType.TryGetValue(concreteType, out var discriminator))
        {
            throw new JsonException($"Unregistered step type: '{concreteType.FullName}'.");
        }

        // Serialize the concrete type to a JSON object, then re-emit it with the
        // discriminator written first.
        using var document = JsonSerializer.SerializeToDocument(value, concreteType, options);

        writer.WriteStartObject();
        writer.WriteString(DiscriminatorName, discriminator);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.NameEquals(DiscriminatorName))
            {
                continue;
            }

            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}
