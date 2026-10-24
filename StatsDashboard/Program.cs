using FantasAIFootball.Utilities;
using StatsDashboard;

var builder = WebApplication.CreateBuilder(args);
var databasePath = builder.Configuration["STATS_DB_PATH"];
if (string.IsNullOrWhiteSpace(databasePath))
    databasePath = Path.Combine(UserData.GetAppDataLocation(), "stats.db");

builder.Services.AddSingleton(new StatsStore(databasePath));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<LeagueNameService>(client =>
{
    client.BaseAddress = new Uri("https://api.sleeper.app/v1/");
    client.Timeout = TimeSpan.FromSeconds(3);
});
var app = builder.Build();
app.UseStaticFiles();

app.MapGet("/", (HttpContext context, StatsStore store, LeagueNameService names, ILogger<Program> logger) =>
    Respond(context, logger, async () =>
    {
        var overview = store.GetOverview(Period.Parse(context.Request.Query["period"]));
        return DashboardPage.Overview(overview, await names.GetNamesAsync(overview.Leagues.Select(league => league.Id)));
    }));

app.MapGet("/league/{leagueId}", (string leagueId, HttpContext context, StatsStore store, LeagueNameService names, ILogger<Program> logger) =>
    Respond(context, logger, async () => DashboardPage.League(
        leagueId, await names.GetNameAsync(leagueId), store.GetLeague(leagueId, Period.Parse(context.Request.Query["period"])))));

app.MapGet("/league/{leagueId}/history", (string leagueId, HttpContext context, StatsStore store, LeagueNameService names, ILogger<Program> logger) =>
    Respond(context, logger, async () => DashboardPage.History(
        leagueId, await names.GetNameAsync(leagueId), store.GetHistory(leagueId, Period.Parse(context.Request.Query["period"]),
            int.TryParse(context.Request.Query["page"], out var page) ? page : 1))));

app.Run();

static async Task<IResult> Respond(HttpContext context, ILogger logger, Func<Task<string>> render)
{
    context.Response.Headers.CacheControl = "no-store";
    try
    {
        return Results.Content(await render(), "text/html; charset=utf-8");
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Unable to load tool call statistics");
        return Results.Content(DashboardPage.Error(), "text/html; charset=utf-8", statusCode: 500);
    }
}
