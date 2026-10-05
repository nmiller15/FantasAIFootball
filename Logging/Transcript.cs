using System.Text;
using System.Text.Json;
using FantasAIFootball.Models.Interactions;

namespace FantasAIFootball.Logging;

public static class Transcript
{
    private const int MaxLineLength = 200;

    public static void Thought(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"✻ {Truncate(Flatten(text))}");
        Console.ResetColor();
    }

    public static void ToolCall(FunctionCallStep call)
    {
        var arguments = string.Join(", ",
            call.Arguments.EnumerateObject()
                .Select(prop => $"{prop.Name}: {RenderValue(prop.Value)}"));

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("● ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(call.Name);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"({Truncate(arguments)})");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void ToolResult(FunctionResultStep result)
    {
        var text = string.Join(" ", result.Result.Select(c => c.Text));

        Console.ForegroundColor = result.IsError ? ConsoleColor.Red : ConsoleColor.DarkGray;
        Console.WriteLine($"  ⎿ {Truncate(Flatten(text))}");
        Console.ResetColor();
    }

    private static string RenderValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => $"\"{element.GetString()}\"",
            JsonValueKind.Object or JsonValueKind.Array => JsonSerializer.Serialize(element),
            JsonValueKind.Null => "null",
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.GetRawText()
        };
    }

    private static string Flatten(string text)
    {
        var builder = new StringBuilder(text.Length);
        var pendingSpace = false;

        foreach (var character in text)
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string Truncate(string text)
    {
        if (text.Length <= MaxLineLength)
        {
            return text;
        }

        return $"{text[..MaxLineLength]} …(+{text.Length - MaxLineLength:N0} chars)";
    }
}
