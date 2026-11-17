using System.Globalization;

namespace StatsDashboard;

public static class DashboardFormatting
{
    private const string FullWidthReportStyles = """
        <style>
            /* The saved email is intentionally constrained to 600px for mail clients.
               Only the full-screen viewer expands its presentation tables. */
            .email-bg, .email-bg > tbody, .email-bg > tbody > tr,
            .email-bg > tbody > tr > td, .container, .container > tbody,
            .container > tbody > tr, .container > tbody > tr > td {
                display: block !important;
                width: 100% !important;
                max-width: none !important;
                box-sizing: border-box !important;
            }
            .email-bg > tbody > tr > td { padding: 0 !important; }
            .container { border-radius: 0 !important; }
            .container > tbody > tr > td { padding: 24px 32px !important; }
        </style>
        """;

    public static string FullWidthReport(string body)
    {
        var headEnd = body.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        return headEnd < 0 ? FullWidthReportStyles + body : body.Insert(headEnd, FullWidthReportStyles);
    }

    public static string Time(string? value) =>
        value is { Length: >= 19 } ? value[..10] + " " + value.Substring(11, 8) : value ?? "—";

    public static string RunTime(string? value, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(value)) return "—";
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return "—";

        var days = DateOnly.FromDateTime(date.Date).DayNumber - today.DayNumber;
        var relative = days switch
        {
            0 => "Today",
            1 => "Tomorrow",
            -1 => "Yesterday",
            > 1 => $"In {days} days",
            _ => $"{-days} days ago"
        };
        var format = date.Year == today.Year ? "ddd, MMM d 'at' h:mm tt" : "ddd, MMM d, yyyy 'at' h:mm tt";
        return relative + " · " + date.ToString(format, CultureInfo.InvariantCulture);
    }
}
