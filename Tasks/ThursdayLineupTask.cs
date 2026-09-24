using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class ThursdayLineupTask : BaseFantasyFootballTask
{
    public override string Name => nameof(ThursdayLineupTask);

    public ThursdayLineupTask(Agent agent, LeagueRepository leagueRepository, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, leagueRepository, promptRepository, emailRepository, configuration)
    {
        var league = leagueRepository.GetLeague().GetAwaiter().GetResult();
        EmailSubject = $"{league?.Name ?? "Unknown league"} - Thursday Lineup Recommendations";
        PromptName = "thursday_checks";
    }
}
