using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class TuesdayWaiverTask : BaseFantasyFootballTask
{
    public override string Name => nameof(TuesdayWaiverTask);

    public TuesdayWaiverTask(Agent agent, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, promptRepository, emailRepository, configuration)
    {
        PromptName = "tuesday_waivers";
        EmailSubject = "Waiver Wire Recommendations";
    }
}
