using FantasAIFootball.Extensions;
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
        builder.Services.AddUtilities();
        builder.Services.AddRepositories();
        builder.Services.AddServices();

        builder.Services.AddTasks();

        var app = builder.Build();

        var firstArg = args.FirstOrDefault();

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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("SUCCESS");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" - ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(matchingTask.Name);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" executed successfully.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            var emailRepository = app.Services.GetRequiredService<EmailRepository>();
            var config = app.Services.GetRequiredService<IConfiguration>();
            var to = config["to"];
            var from = config["from"];
            var subject = $"ERROR executing task {matchingTask.Name}: {ex.Message}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR executing task {matchingTask.Name}: {ex.Message}");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" - ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(matchingTask.Name);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" could not be executed.");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Message: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(ex.Message);
            Console.ResetColor();

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
