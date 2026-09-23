using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class SundayLineupTask : BaseFantasyFootballTask
{
    public override string Name => nameof(SundayLineupTask);

    public SundayLineupTask(Agent agent, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, promptRepository, emailRepository, configuration)
    {
        PromptName = "sunday_lineup";
        EmailSubject = "Sunday Lineup Recommendations";
    }
}
