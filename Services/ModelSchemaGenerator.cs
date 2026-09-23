using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace FantasAIFootball.Utilities;

/// <summary>
/// Generates compact, human-readable schema documentation for model types by
/// reflecting over their <see cref="JsonPropertyNameAttribute"/> definitions.
/// Output uses the JSON field names and JSON-ish type names that the agent
/// actually observes in serialized tool results, so the docs never drift from
/// the code.
/// </summary>
public static class ModelSchemaGenerator
{
    private const string ModelsNamespacePrefix = "FantasAIFootball.Models";

    /// <summary>
    /// Builds a documentation block describing each supplied type. Nested model
    /// types (e.g. ScoringSettings referenced by League) are discovered
    /// automatically and documented once each.
    /// </summary>
    public static string Generate(params Type[] types)
    {
        var documented = new HashSet<Type>();
        var queue = new Queue<Type>(types);
        var builder = new StringBuilder();

        while (queue.Count > 0)
        {
            var type = queue.Dequeue();
            if (!documented.Add(type))
            {
                continue;
            }

            builder.Append(type.Name).Append(": { ");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var fieldName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? property.Name;

                builder.Append(fieldName).Append(": ").Append(MapType(property.PropertyType, queue));

                if (i < properties.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.AppendLine(" }").AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string MapType(Type type, Queue<Type> nestedTypes)
    {
        // Unwrap Nullable<T> (e.g. int? -> int).
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            return MapType(underlying, nestedTypes) + "?";
        }

        // Collections -> elementType[].
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
        {
            var element = GetEnumerableElementType(type);
            return element != null
                ? MapType(element, nestedTypes) + "[]"
                : "array";
        }

        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)) return "number";
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "string";

        // Nested model type: reference it by name and queue it for documentation.
        if (type.Namespace != null && type.Namespace.StartsWith(ModelsNamespacePrefix))
        {
            nestedTypes.Enqueue(type);
            return type.Name;
        }

        return type.Name;
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments().FirstOrDefault();
        }

        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments().FirstOrDefault();
    }
}
