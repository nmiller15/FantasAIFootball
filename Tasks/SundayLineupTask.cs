using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class SundayLineupTask : BaseFantasyFootballTask
{
    public override string Name => nameof(SundayLineupTask);

    public SundayLineupTask(Agent agent, LeagueRepository leagueRepository, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, leagueRepository, promptRepository, emailRepository, configuration)
    {
        var league = leagueRepository.GetLeague().GetAwaiter().GetResult();
        EmailSubject = $"{league?.Name ?? "Unknown league"} - Sunday Lineup Recommendations";
        PromptName = "sunday_lineup";
    }
}
