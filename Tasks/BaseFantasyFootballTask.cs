using System.Text.RegularExpressions;
using FantasAIFootball.Extensions;
using FantasAIFootball.Models.Email;
using FantasAIFootball.Repositories;
using FantasAIFootball.Services;
using FantasAIFootball.Utilities;
using Markdig;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Tasks;

public class BaseFantasyFootballTask : ITask
{
    private static readonly Regex AnsiEscape = new(@"\x1b\[[0-9;]*m", RegexOptions.Compiled);

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
        _leagueRepository = leagueRepository;
        _promptRepository = promptRepository;
        _emailRepository = emailRepository;

        _emailTo = configuration["to"] ?? throw new Exception("'to' not found in configuration");
        _emailFrom = configuration["from"] ?? throw new Exception("'from' not found in configuration");
    }

    /// <summary>
    /// Returns the subject line for the report. Overridden by tasks whose subject
    /// depends on runtime state, such as the current day.
    /// </summary>
    public virtual string BuildSubject() => EmailSubject;

    /// <summary>
    /// Gives the task a chance to prepend runtime context to the prompt loaded from
    /// Prompts before it is handed to the agent.
    /// </summary>
    protected virtual Task<string> ComposePromptAsync(string prompt) => Task.FromResult(prompt);

    public async Task Execute()
    {
        var output = Console.Out;
        var capture = new StringWriter();

        var teeWriter = new TeeWriter(output, capture);
        Console.SetOut(teeWriter);

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

        var composedPrompt = await ComposePromptAsync(prompt);

        var result = await _agent.StartAgent(composedPrompt);
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

        var emailSubject = BuildSubject();

        var contentHtml = Markdown.ToHtml(content);
        var consoleHtml = System.Net.WebUtility.HtmlEncode(AnsiEscape.Replace(capture.ToString(), ""));

        var emailBody =
                contentHtml +
                "<hr style=\"margin-top: 24px; border: none; border-top: 1px solid #ccc;\" />" +
                "<div style=\"margin-top: 16px;\">" +
                    "<h3 style=\"font-family: sans-serif; color: #333; margin-bottom: 8px;\">Console Output</h3>" +
                    "<pre style=\"" +
                        "background-color: #1e1e1e; color: #d4d4d4; " +
                        "padding: 16px; border-radius: 6px; " +
                        "font-family: 'Consolas', 'Menlo', monospace; font-size: 13px; " +
                        "line-height: 1.4; overflow-x: auto; white-space: pre-wrap; " +
                        "word-break: break-word;\">" +
                        consoleHtml +
                    "</pre>" +
                "</div>";

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
