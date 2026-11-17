using System.Globalization;

namespace StatsDashboard;

public static class DashboardFormatting
{
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
