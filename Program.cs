using FantasAIFootball.Extensions;
using FantasAIFootball.Logging;
using FantasAIFootball.Models.Email;
using FantasAIFootball.Repositories;
using FantasAIFootball.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FantasAIFootball;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.LoadConfiguration();

#if DEBUG
        Log.MinLevel = LogLevel.Debug;
#else
        Log.MinLevel = builder.Configuration.GetValue<bool>("debug") ? LogLevel.Debug : LogLevel.Info;
#endif

        builder.Services.AddUtilities();
        builder.Services.AddRepositories();
        builder.Services.AddServices();

        builder.Services.AddTasks();

        var firstArg = args.FirstOrDefault();
        if (args.Length > 1)
        {
            var leagueId = args[1];
            if (!string.IsNullOrEmpty(leagueId))
            {
                builder.Configuration["leagueId"] = leagueId;
            }
        }

        var app = builder.Build();

        var tasks = app.Services.GetRequiredService<IEnumerable<ITask>>();

        var matchingTask = tasks.FirstOrDefault(t => t.Name.Equals(firstArg ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        if (matchingTask == null)
        {
            Log.Error($"Task '{firstArg ?? string.Empty}' could not be found.");
            Log.Info("Available tasks:");
            foreach (var task in tasks)
            {
                Log.Info($"- {task.Name}");
            }

            return;
        }

        try
        {
            await matchingTask.Execute();

            Log.Info($"{matchingTask.Name} executed successfully.");
        }
        catch (Exception ex)
        {
            var emailRepository = app.Services.GetRequiredService<EmailRepository>();
            var config = app.Services.GetRequiredService<IConfiguration>();
            var to = config["to"];
            var from = config["from"];
            var subject = $"ERROR executing task {matchingTask.Name}: {ex.Message}";

            Log.Error($"Error executing task {matchingTask.Name}: {ex.Message}", ex);

            var email = new Email
            {
                To = to,
                From = from,
                Subject = subject,
                Body = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}"
            };

            await emailRepository.SendEmail(email);
        }
    }
}
