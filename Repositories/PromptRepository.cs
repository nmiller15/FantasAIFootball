using FantasAIFootball.Models.League;
using FantasAIFootball.Models.Memories;
using FantasAIFootball.Models.Search;
using FantasAIFootball.Utilities;

namespace FantasAIFootball.Repositories;

public class PromptRepository
{
    public PromptRepository()
    {
    }

    public async Task<string> GetPrompt(string promptName)
    {
        var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", $"{promptName}.txt");
        if (!File.Exists(promptPath))
        {
            throw new FileNotFoundException($"Prompt file '{promptName}.txt' not found in Prompts directory.");
        }

        return await File.ReadAllTextAsync(promptPath);
    }

    public async Task<string> GetSystemInstruction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "system_prompt.txt");
        var modelDocs = ModelSchemaGenerator.Generate(
            typeof(League),
            typeof(ScoringSettings),
            typeof(Roster),
            typeof(Matchup),
            typeof(Player),
            typeof(User),
            typeof(NflState),
            typeof(QueryResult),
            typeof(ExtractResult),
            typeof(Memory),
            typeof(Recommendation));

        var systemInstruction = await File.ReadAllTextAsync(path);

        return systemInstruction + "\n" + modelDocs;
    }
}
