using Microsoft.AspNetCore.Mvc;

namespace StatsDashboard.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DashboardController(StatsStore store, RunStore runs, LeagueNameService names,
    ILogger<DashboardController> logger) : Controller
{
    [HttpGet("/")]
    public Task<IActionResult> Overview([FromQuery] string? period) => Load(async () =>
    {
        var range = Period.Parse(period);
        var overview = store.GetOverview(range);
        var leagues = overview.Leagues.ToList();
        var known = leagues.Select(league => league.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var id in runs.GetLeagueIds(range))
            if (known.Add(id)) leagues.Add(new LeagueSummary(id, 0, 0, null));
        overview = overview with { Leagues = leagues };
        return View(new OverviewViewModel(overview,
            await names.GetNamesAsync(leagues.Select(league => league.Id)), runs.GetRunTimings()));
    });

    [HttpGet("/league/{leagueId}")]
    public Task<IActionResult> League(string leagueId, [FromQuery] string? period) => Load(async () =>
    {
        var range = Period.Parse(period);
        return View(new LeagueViewModel(leagueId, await names.GetNameAsync(leagueId),
            store.GetLeague(leagueId, range), runs.GetRuns(leagueId, range, pageSize: 5)));
    });

    [HttpPost("/league/{leagueId}/runs")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ScheduleRun(string leagueId, [FromForm] string? notes, [FromForm] string? period) => Load(async () =>
    {
        var range = Period.Parse(period);
        if (notes?.Length > 2000)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View("League", new LeagueViewModel(leagueId, await names.GetNameAsync(leagueId),
                store.GetLeague(leagueId, range), runs.GetRuns(leagueId, range, pageSize: 5),
                "Notes must be 2,000 characters or fewer."));
        }

        var all = Period.Parse("all");
        if (!store.GetOverview(all).Leagues.Any(league => league.Id == leagueId) &&
            !runs.GetLeagueIds(all).Contains(leagueId, StringComparer.Ordinal))
            return NotFound();

        await runs.ScheduleRunAsync(leagueId, notes ?? "");
        TempData["ScheduledRun"] = "Run scheduled for the next tick.";
        return RedirectToAction(nameof(League), new { leagueId, period = range.Query })!;
    });

    [HttpGet("/league/{leagueId}/history")]
    public Task<IActionResult> History(string leagueId, [FromQuery] string? period, [FromQuery] string? page) => Load(async () =>
        View(new HistoryViewModel(leagueId, await names.GetNameAsync(leagueId),
            store.GetHistory(leagueId, Period.Parse(period), ParsePage(page)))));

    [HttpGet("/league/{leagueId}/runs")]
    public Task<IActionResult> Runs(string leagueId, [FromQuery] string? period, [FromQuery] string? page) => Load(async () =>
        View(new RunsViewModel(leagueId, await names.GetNameAsync(leagueId),
            runs.GetRuns(leagueId, Period.Parse(period), ParsePage(page)))));

    [HttpGet("/league/{leagueId}/runs/{key:int}/report")]
    public Task<IActionResult> Report(string leagueId, int key) => Load(() =>
    {
        var report = runs.GetReport(leagueId, key);
        return Task.FromResult<IActionResult>(report is null ? NotFound() : View(report));
    });

    private static int ParsePage(string? value) => int.TryParse(value, out var page) ? page : 1;

    private async Task<IActionResult> Load(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to load dashboard data");
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("Error", new ErrorViewModel());
        }
    }
}
