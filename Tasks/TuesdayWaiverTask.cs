using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class TuesdayWaiverTask : BaseFantasyFootballTask
{
    public override string Name => nameof(TuesdayWaiverTask);

    public TuesdayWaiverTask(Agent agent, LeagueRepository leagueRepository, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, leagueRepository, promptRepository, emailRepository, configuration)
    {
        var league = leagueRepository.GetLeague().GetAwaiter().GetResult();
        EmailSubject = $"{league?.Name ?? "Unknown league"} - Waiver Wire Recommendations";
        PromptName = "tuesday_waivers";
    }
}
