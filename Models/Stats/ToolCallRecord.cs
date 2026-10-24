namespace FantasAIFootball.Models.Stats;

public class ToolCallRecord
{
    public int Key { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public DateTime InsertedAt { get; set; }
}
