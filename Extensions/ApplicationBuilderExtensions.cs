using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using FantasAIFootball.Utilities;
using FantasAIFootball.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FantasAIFootball.Extensions;

public static class ApplicationBuilderExtensions
{
    public static HostApplicationBuilder LoadConfiguration(this HostApplicationBuilder builder)
    {
        var appDataFolder = UserData.GetAppDataLocation();

        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
            File.WriteAllText(Path.Combine(appDataFolder, "api_keys.json"),
                """
                {
                    "tavilyKey": "",
                    "geminiKey": ""
                }
                """);

            File.WriteAllText(Path.Combine(appDataFolder, "ai_config.json"),
                """
                {
                    "model": ""
                }
                """);

            File.WriteAllText(Path.Combine(appDataFolder, "league_config.json"),
                """
                {
                    "username": "",
                    "leagueId": ""
                }
                """);

            File.WriteAllText(Path.Combine(appDataFolder, "email_config.json"),
                """
                {
                    "smtpUser": "",
                    "smtpPassword": "",
                    "smtpServer": "",
                    "smtpPort": 587,
                    "to": "",
                    "from": ""
                }
                """);

            File.WriteAllText(Path.Combine(appDataFolder, "appsettings.json"),
                """
                {
                    "debug": false
                }
                """);
        }

        try
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(appDataFolder, "api_keys.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(appDataFolder, "ai_config.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(appDataFolder, "league_config.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(appDataFolder, "email_config.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(appDataFolder, "appsettings.json"), optional: true, reloadOnChange: true)
                .Build();

            builder.Configuration.AddConfiguration(config);

            return builder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            Console.WriteLine("Please ensure that the configuration files exist in the following directory:");
            Console.WriteLine(appDataFolder);
            Console.WriteLine($"Required files: api_keys.json, ai_config.json, league_config.json");

            throw;
        }
    }

    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddSingleton<PlayerCache>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<EmailRepository>();
        services.AddSingleton<LeagueRepository>();
        services.AddSingleton<MemoryRepository>();
        services.AddSingleton<WebSearchRepository>();
        services.AddSingleton<InteractionsRepository>();
        services.AddSingleton<PromptRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<FunctionService>();
        services.AddSingleton<Agent>();

        return services;
    }

    public static IServiceCollection AddTasks(this IServiceCollection services)
    {
        services.AddSingleton<ITask, TuesdayWaiverTask>();
        services.AddSingleton<ITask, ThursdayLineupTask>();
        services.AddSingleton<ITask, MondayLineupTask>();
        services.AddSingleton<ITask, SundayLineupTask>();

        return services;
    }
}
