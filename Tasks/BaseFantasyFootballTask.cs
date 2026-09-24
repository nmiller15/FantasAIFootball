using FantasAIFootball.Extensions;
using FantasAIFootball.Models.Email;
using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using Markdig;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class BaseFantasyFootballTask : ITask
{
    protected readonly Agent _agent;
    protected readonly LeagueRepository _leagueRepository;
    protected readonly PromptRepository _promptRepository;
    protected readonly EmailRepository _emailRepository;

    protected readonly string _emailTo;
    protected readonly string _emailFrom;

    public virtual string Name { get; }
    public string PromptName { get; set; }
    public string EmailSubject { get; set; }

    public BaseFantasyFootballTask(Agent agent, LeagueRepository leagueRepository, PromptRepository promptRepository, EmailRepository emailRepository, IConfiguration configuration)
    {
        _agent = agent;
        _promptRepository = promptRepository;
        _emailRepository = emailRepository;

        _emailTo = configuration["to"] ?? throw new Exception("'to' not found in configuration");
        _emailFrom = configuration["from"] ?? throw new Exception("'from' not found in configuration");
    }

    public async Task Execute()
    {
        var systemInstruction = await _promptRepository.GetSystemInstruction();
        if (string.IsNullOrEmpty(systemInstruction))
        {
            throw new Exception("System instruction not found");
        }

        _agent.AddSystemInstruction(systemInstruction);
        _agent.RegisterTools();

        var prompt = await _promptRepository.GetPrompt(PromptName);
        if (string.IsNullOrEmpty(prompt))
        {
            throw new Exception("Prompt not found");
        }

        var result = await _agent.StartAgent(prompt);
        var contentList = result?.Content.Select(c => c.Text).ToList();
        var content = contentList != null ? string.Join("\n", contentList) : null;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Agent output:");
        Console.ResetColor();
        Console.WriteLine(Markdown.Normalize(content));

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("Agent failed to produce output.");
        }

        var emailSubject = EmailSubject;
        var emailBody = Markdown.ToHtml(content);

        var email = new Email
        {
            To = _emailTo,
            From = _emailFrom,
            Subject = emailSubject,
            Body = emailBody,
        };

        await _emailRepository.SendEmail(email);
    }
}
