using FantasAIFootball.Utilities;
using StatsDashboard;

var builder = WebApplication.CreateBuilder(args);
var databasePath = builder.Configuration["STATS_DB_PATH"];
if (string.IsNullOrWhiteSpace(databasePath))
    databasePath = Path.Combine(UserData.GetAppDataLocation(), "stats.db");
var schedulePath = builder.Configuration["SCHEDULE_DB_PATH"];
if (string.IsNullOrWhiteSpace(schedulePath))
    schedulePath = Path.Combine(UserData.GetAppDataLocation(), "schedule.db");

builder.Services.AddSingleton(new StatsStore(databasePath));
builder.Services.AddSingleton(new RunStore(schedulePath));
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<LeagueNameService>(client =>
{
    client.BaseAddress = new Uri("https://api.sleeper.app/v1/");
    client.Timeout = TimeSpan.FromSeconds(3);
});
var app = builder.Build();
app.UseStaticFiles();
app.MapControllers();

app.Run();
