using FantasAIFootball.Models.League;
using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class DailyCheckTask : BaseFantasyFootballTask
{
    public override string Name => nameof(DailyCheckTask);

    private const string ContextPlaceholder = "{{RUN_CONTEXT}}";

    private readonly IConfiguration _configuration;

    private League? _league;
    private string? _leagueName;

    public DailyCheckTask(Agent agent, LeagueRepository leagueRepository, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
        : base(agent, leagueRepository, promptRepository, emailRepository, configuration)
    {
        _configuration = configuration;
        PromptName = "daily_check";
    }

    public override string BuildSubject()
    {
        _league ??= _leagueRepository.GetLeague().GetAwaiter().GetResult();
        _leagueName ??= _league?.Name ?? "Unknown league";

        return $"{_leagueName} - Daily Check ({DateTime.Now:ddd MMM d})";
    }

    protected override async Task<string> ComposePromptAsync(string prompt)
    {
        var nflState = await _leagueRepository.GetNflState();

        _league ??= await _leagueRepository.GetLeague();

        // Optional override, used only if the league's own settings are missing or
        // if Sleeper's waiver_day_of_week convention ever needs pinning.
        var dayOverride = _configuration.GetValue<int?>("waiverDayOfWeek");

        var runContext = RunContextBuilder.Build(
            DateTimeOffset.Now,
            nflState,
            _league?.Settings,
            dayOverride);

        if (string.IsNullOrWhiteSpace(runContext))
        {
            throw new Exception("Failed to build run context.");
        }

        return prompt.Contains(ContextPlaceholder, StringComparison.Ordinal)
            ? prompt.Replace(ContextPlaceholder, runContext)
            : runContext + Environment.NewLine + Environment.NewLine + prompt;
    }
}