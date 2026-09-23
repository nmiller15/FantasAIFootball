using System.Net;
using FantasAIFootball.Models.Email;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace FantasAIFootball.Repositories;

public class EmailRepository
{
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _smtpServer;
    private readonly int _smtpPort;

    public EmailRepository(IConfiguration configuration)
    {
        _smtpUser = configuration.GetValue<string>("smtpUser") ?? throw new Exception("SMTP user not found in configuration.");
        _smtpPassword = configuration.GetValue<string>("smtpPassword") ?? throw new Exception("SMTP password not found in configuration.");
        _smtpServer = configuration.GetValue<string>("smtpServer") ?? throw new Exception("SMTP server not found in configuration.");
        _smtpPort = configuration.GetValue<int?>("smtpPort") ?? throw new Exception("SMTP port not found in configuration.");
    }

    public async Task<bool> SendEmail(Email email)
    {
        NetworkCredential credentials = new()
        {
            UserName = _smtpUser,
            Password = _smtpPassword
        };

        using SmtpClient client = new();
        try
        {
            await client.ConnectAsync(
                    _smtpServer,
                    _smtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                );
            await client.AuthenticateAsync(credentials);

            var message = new MimeMessage()
            {
                Subject = email.Subject,
                Body = new BodyBuilder() { HtmlBody = email.Body, }.ToMessageBody(),
            };

            message.From.Add(new MailboxAddress(email.From, email.From));
            message.To.Add(new MailboxAddress(email.To, email.To));

            await client.SendAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            return false;
        }
    }
}
