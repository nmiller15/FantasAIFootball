using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class MondayLineupTask : BaseFantasyFootballTask
{
    public override string Name => nameof(MondayLineupTask);

    public MondayLineupTask(Agent agent, LeagueRepository leagueRepository, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, leagueRepository, promptRepository, emailRepository, configuration)
    {
        var league = leagueRepository.GetLeague().GetAwaiter().GetResult();
        EmailSubject = $"{league?.Name ?? "Unknown league"} - Monday Lineup Recommendations";
        PromptName = "monday_checks";
    }
}
