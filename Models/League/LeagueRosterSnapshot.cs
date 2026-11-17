namespace FantasAIFootball.Models.League;

public class LeagueRosterSnapshot
{
    public int RosterId { get; set; }
    public string OwnerId { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string TeamName { get; set; } = "";
    public List<LeagueRosterPlayer> Players { get; set; } = [];
}

public class LeagueRosterPlayer
{
    public string PlayerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";
    public string Slot { get; set; } = "";
}
