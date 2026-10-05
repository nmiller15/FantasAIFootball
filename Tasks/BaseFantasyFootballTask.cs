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

        var emailBody = BuildEmailBody(emailSubject, contentHtml, consoleHtml);

        var email = new Email
        {
            To = _emailTo,
            From = _emailFrom,
            Subject = emailSubject,
            Body = emailBody,
        };

        await _emailRepository.SendEmail(email);
    }

    /// <summary>
    /// Wraps the rendered report in a full HTML document with a viewport meta tag,
    /// a fluid 600px-max container, and media queries so it reads well on phone
    /// clients such as Spark on iOS.
    /// </summary>
    private static string BuildEmailBody(string subject, string contentHtml, string consoleHtml)
    {
        var subjectHtml = System.Net.WebUtility.HtmlEncode(subject);

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <meta name="color-scheme" content="light dark" />
              <meta http-equiv="X-UA-Compatible" content="IE=edge" />
              <title>{{subjectHtml}}</title>
              <style type="text/css">
                body {
                  margin: 0;
                  padding: 0;
                  width: 100% !important;
                  background-color: #f4f5f7;
                  -webkit-text-size-adjust: 100%;
                  -ms-text-size-adjust: 100%;
                }
                table { border-collapse: collapse; }
                a { color: #0969da; }
                img {
                  max-width: 100%;
                  height: auto;
                  border: 0;
                  outline: none;
                  text-decoration: none;
                  -ms-interpolation-mode: bicubic;
                }
                .report {
                  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;
                  font-size: 16px;
                  line-height: 1.55;
                  color: #24292f;
                  word-wrap: break-word;
                  overflow-wrap: break-word;
                }
                .report h1, .report h2, .report h3, .report h4, .report h5, .report h6 {
                  margin: 24px 0 12px;
                  font-weight: 600;
                  line-height: 1.25;
                  word-wrap: break-word;
                }
                .report h1 { font-size: 26px; }
                .report h2 { font-size: 22px; }
                .report h3 { font-size: 19px; }
                .report h4, .report h5, .report h6 { font-size: 16px; }
                .report p, .report ul, .report ol, .report pre,
                .report blockquote, .report table { margin: 0 0 16px; }
                .report ul, .report ol { padding-left: 24px; }
                .report li { margin-bottom: 6px; }
                .report li > p { margin-bottom: 8px; }
                .report blockquote {
                  padding: 4px 16px;
                  border-left: 4px solid #d0d7de;
                  color: #57606a;
                }
                .report code {
                  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, "Liberation Mono", monospace;
                  font-size: 13px;
                  background-color: #f6f8fa;
                  padding: 2px 5px;
                  border-radius: 4px;
                  word-break: break-all;
                }
                .report pre {
                  background-color: #1e1e1e;
                  color: #d4d4d4;
                  padding: 16px;
                  border-radius: 6px;
                  font-size: 13px;
                  line-height: 1.45;
                  overflow-x: auto;
                  -webkit-overflow-scrolling: touch;
                  white-space: pre-wrap;
                  word-break: break-word;
                }
                .report pre code {
                  background: none;
                  padding: 0;
                  font-size: inherit;
                  color: inherit;
                  word-break: normal;
                }
                .report table {
                  width: 100%;
                  display: block;
                  overflow-x: auto;
                  -webkit-overflow-scrolling: touch;
                  font-size: 14px;
                }
                .report th, .report td {
                  border: 1px solid #d0d7de;
                  padding: 6px 10px;
                  text-align: left;
                  word-break: break-word;
                }
                .report th { background-color: #f6f8fa; }
                .report hr {
                  border: none;
                  border-top: 1px solid #d8dee4;
                  margin: 24px 0;
                }
                .console-title {
                  font-size: 17px;
                }
                @media only screen and (max-width: 620px) {
                  .container { width: 100% !important; border-radius: 0 !important; }
                  .report { font-size: 15px !important; }
                  .report h1 { font-size: 23px !important; margin-top: 20px !important; }
                  .report h2 { font-size: 20px !important; margin-top: 20px !important; }
                  .report h3 { font-size: 18px !important; margin-top: 20px !important; }
                  .report pre { padding: 12px !important; font-size: 12px !important; }
                  .report ul, .report ol { padding-left: 20px !important; }
                }
                @media (prefers-color-scheme: dark) {
                  body { background-color: #0d1117 !important; }
                  .email-bg { background-color: #0d1117 !important; }
                  .container { background-color: #161b22 !important; }
                  .report { color: #e6edf3 !important; }
                  a { color: #58a6ff !important; }
                  .console-title { color: #e6edf3 !important; }
                  .report blockquote {
                    color: #9da7b3 !important;
                    border-left-color: #30363d !important;
                  }
                  .report code { background-color: #22272e !important; color: #e6edf3 !important; }
                  .report pre {
                    background-color: #010409 !important;
                    color: #d4d4d4 !important;
                    border: 1px solid #30363d;
                  }
                  .report pre code { background: none !important; color: inherit !important; }
                  .report th, .report td { border-color: #30363d !important; }
                  .report th { background-color: #21262d !important; }
                  .report hr { border-top-color: #30363d !important; }
                }
              </style>
            </head>
            <body style="margin: 0; padding: 0; background-color: #f4f5f7; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%;">
              <table role="presentation" class="email-bg" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color: #f4f5f7;">
                <tr>
                  <td align="center" style="padding: 16px 8px;">
                    <table role="presentation" class="container" width="100%" cellpadding="0" cellspacing="0" border="0"
                           style="max-width: 600px; background-color: #ffffff; border-radius: 10px;">
                      <tr>
                        <td style="padding: 24px 20px;">
                          <div class="report">
                            {{contentHtml}}
                            <hr />
                            <p class="console-title" style="font-weight: 600; color: #24292f; margin: 0 0 8px;">Console Output</p>
                            <pre style="background-color: #1e1e1e; color: #d4d4d4; padding: 16px; border-radius: 6px; font-family: 'Consolas', 'Menlo', monospace; font-size: 13px; line-height: 1.4; overflow-x: auto; -webkit-overflow-scrolling: touch; white-space: pre-wrap; word-break: break-word;">{{consoleHtml}}</pre>
                          </div>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
