using FantasAIFootball.Extensions;
using FantasAIFootball.Models.Email;
using FantasAIFootball.Repositories;
using FantasAIFootball.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FantasAIFootball;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
#if !DEBUG
            .MinimumLevel.Information()
#endif
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Level:u3} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);

        builder.LoadConfiguration();
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" - ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Task ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"'{firstArg ?? string.Empty}'");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" could not be found.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Available tasks:");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var task in tasks)
            {
                Console.WriteLine($"- {task.Name}");
            }

            Console.ResetColor();
            return;
        }

        try
        {
            await matchingTask.Execute();

            Log.Information($"SUCCESS - {matchingTask.Name} executed successfully.");
        }
        catch (Exception ex)
        {
            var emailRepository = app.Services.GetRequiredService<EmailRepository>();
            var config = app.Services.GetRequiredService<IConfiguration>();
            var to = config["to"];
            var from = config["from"];
            var subject = $"ERROR executing task {matchingTask.Name}: {ex.Message}";

            Log.Error(ex, $"ERROR executing task {matchingTask.Name}: {ex.Message}");

            var email = new Email
            {
                To = to,
                From = from,
                Subject = subject,
                Body = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}"
            };

            await emailRepository.SendEmail(email);

            Log.CloseAndFlush();
        }
    }
}
