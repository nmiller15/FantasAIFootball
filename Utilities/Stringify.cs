using System.Text.Json;

namespace FantasAIFootball.Utilities;

public static class Stringify
{
    public static string Model(object model)
    {
        return JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
    }
}
