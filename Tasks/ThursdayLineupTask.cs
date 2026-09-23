using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class ThursdayLineupTask : BaseFantasyFootballTask
{
    public override string Name => nameof(ThursdayLineupTask);

    public ThursdayLineupTask(Agent agent, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, promptRepository, emailRepository, configuration)
    {
        PromptName = "thursday_checks";
        EmailSubject = "Thursday Lineup Recommendations";
    }
}
