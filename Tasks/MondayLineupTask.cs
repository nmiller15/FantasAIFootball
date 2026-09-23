using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class MondayLineupTask : BaseFantasyFootballTask
{
    public override string Name => nameof(MondayLineupTask);

    public MondayLineupTask(Agent agent, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, promptRepository, emailRepository, configuration)
    {
        PromptName = "monday_checks";
        EmailSubject = "Monday Lineup Recommendations";
    }
}
