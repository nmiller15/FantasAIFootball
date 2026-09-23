using System.Text;

namespace FantasAIFootball.Models.Email;

public class Email
{
    public string To { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }

    public string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine("<Email>");
        builder.AppendLine($"|  From: {From}");
        builder.AppendLine($"|  To: {To}");
        builder.AppendLine($"|  Subject: {Subject}");
        builder.AppendLine($"|  Body: {Body}");

        return builder.ToString();
    }
}
