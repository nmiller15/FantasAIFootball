using System.Net;
using System.Globalization;
using System.Text;

namespace StatsDashboard;

public static class DashboardPage
{
    private static string H(string? value) => WebUtility.HtmlEncode(value ?? "");
    private static string Link(string id, Period period) => "/league/" + Uri.EscapeDataString(id) + "?period=" + period.Query;
    private static string HistoryLink(string id, Period period) => "/league/" + Uri.EscapeDataString(id) + "/history?period=" + period.Query;
    private static string RunsLink(string id, Period period) => "/league/" + Uri.EscapeDataString(id) + "/runs?period=" + period.Query;

    private static StringBuilder Start(string title, Period? period = null, string? leagueId = null,
        string? leagueName = null, bool history = false, bool runs = false)
    {
        var page = new StringBuilder("""
            <!doctype html><html lang="en"><head><meta charset="utf-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <meta name="robots" content="noindex,nofollow">
            <title>FantasAI Football Dashboard</title><link rel="stylesheet" href="/style.css"><link rel="stylesheet" href="/calls.css"></head><body>
            <div class="shell"><header class="header"><a class="brand" href="/">FANTAS<span>AI</span>FOOTBALL <small>/ STATS</small></a>
            <span class="header-note">LEAGUE ACTIVITY</span></header><main>
            """);
        if (leagueId is not null)
        {
            page.Append("<nav class=\"breadcrumb\"><a href=\"/?period=").Append(period?.Query)
                .Append("\">ALL LEAGUES</a> <span>/</span> ");
            if (history || runs)
                page.Append("<a href=\"").Append(H(Link(leagueId, period!.Value))).Append("\">")
                    .Append(H(leagueName ?? leagueId)).Append("</a> <span>/</span> ")
                    .Append(runs ? "RUNS" : "HISTORY");
            else
                page.Append(H(leagueName ?? leagueId));
            page.Append("</nav>");
        }

        page.Append("<div class=\"intro\"><div><p class=\"eyebrow\">LIVE LEAGUE ACTIVITY</p><h1>")
            .Append(H(title)).Append("</h1>");
        if (leagueId is not null)
            page.Append("<p class=\"headline-id\">LEAGUE ID · ").Append(H(leagueId)).Append("</p>");
        page.Append("<p class=\"subhead\">Runs and tool activity, organized by league. Times are recorded in the writer’s local time.</p></div>");
        if (period is not null)
        {
            page.Append("<nav class=\"periods\" aria-label=\"Date range\">");
            foreach (var choice in new[] { Period.Parse("7"), Period.Parse("30"), Period.Parse("all") })
            {
                var url = leagueId is null ? "/?period=" + choice.Query
                    : runs ? RunsLink(leagueId, choice) : history ? HistoryLink(leagueId, choice) : Link(leagueId, choice);
                page.Append("<a href=\"").Append(H(url)).Append("\" class=\"")
                    .Append(choice.Query == period.Value.Query ? "active" : "")
                    .Append("\">").Append(H(choice.Label)).Append("</a>");
            }
            page.Append("</nav>");
        }
        page.Append("</div>");
        return page;
    }

    private static string Finish(StringBuilder page) => page.Append("""
        </main><footer>FANTASAIFOOTBALL · COUNTS INCLUDE ATTEMPTED TOOL CALLS · <a href="">REFRESH DATA ↗</a></footer></div>
        </body></html>
        """).ToString();

    private static void Empty(StringBuilder page, bool databaseExists)
    {
        page.Append("<section class=\"empty\"><span class=\"empty-icon\">◇</span><h2>No calls to show</h2><p>")
            .Append(databaseExists
                ? "There are no tool calls in this date range. Try All time or check back later."
                : "The stats database was not found in this user’s app-data directory. Check the writer’s location or set STATS_DB_PATH.")
            .Append("</p></section>");
    }

    public static string Overview(OverviewData data, IReadOnlyDictionary<string, string> names,
        IReadOnlyDictionary<string, RunTiming> timings)
    {
        var page = Start("The league overview", data.Period);
        var today = DateOnly.FromDateTime(DateTime.Today);
        page.Append("<section class=\"metrics\"><div class=\"metric\"><span>TOTAL TOOL CALLS</span><strong>")
            .Append(data.Total.ToString("N0")).Append("</strong><small>").Append(H(data.Period.Label))
            .Append("</small></div><div class=\"metric\"><span>ACTIVE LEAGUES</span><strong>")
            .Append(data.Leagues.Count.ToString("N0"))
             .Append("</strong><small>With recorded activity</small></div></section>");

        page.Append("<div class=\"section-title\"><h2>Leagues</h2><span>")
            .Append(data.Leagues.Count).Append(" LEAGUES</span></div>");
        if (data.Leagues.Count == 0) Empty(page, data.DatabaseExists);
        else
        {
            page.Append("<div class=\"league-grid\">");
            foreach (var league in data.Leagues)
            {
                page.Append("<a class=\"league-card\" href=\"").Append(H(Link(league.Id, data.Period)))
                    .Append("\"><span class=\"card-label\">LEAGUE <span>↗</span></span><h3>")
                    .Append(H(names.TryGetValue(league.Id, out var name) && !string.IsNullOrWhiteSpace(name)
                        ? name : string.IsNullOrEmpty(league.Id) ? "(empty ID)" : league.Id))
                    .Append("</h3><span class=\"league-id\">ID · ").Append(H(league.Id))
                    .Append("</span><div class=\"card-stats\"><div><strong>").Append(league.Calls.ToString("N0"))
                    .Append("</strong><small>CALLS</small></div><div><strong>").Append(league.Tools.ToString("N0"))
                    .Append("</strong><small>TOOLS USED</small></div></div><div class=\"card-run-times\"><p>LAST EXECUTED · ")
                    .Append(H(FormatRunTime(timings.TryGetValue(league.Id, out var timing) ? timing.LastExecuted : null, today)))
                    .Append("</p><p>NEXT SCHEDULED · ")
                    .Append(H(FormatRunTime(timing?.NextScheduled, today)))
                    .Append("</p></div><p>LAST CALL · ")
                    .Append(H(FormatTime(league.LastCall))).Append("</p></a>");
            }
            page.Append("</div>");
        }
        return Finish(page);
    }

    public static string League(string leagueId, string leagueName, LeagueData data, RunsData runs)
    {
        var page = Start(string.IsNullOrWhiteSpace(leagueName) ? leagueId : leagueName, data.Period, leagueId, leagueName);
        page.Append("<section class=\"metrics\"><div class=\"metric\"><span>SCHEDULED RUNS</span><strong>")
            .Append(runs.Total.ToString("N0")).Append("</strong><small>").Append(H(data.Period.Label))
            .Append("</small></div><div class=\"metric\"><span>TOTAL TOOL CALLS</span><strong>")
            .Append(data.Total.ToString("N0")).Append("</strong><small>")
            .Append(H(data.Period.Label)).Append("</small></div></section>");

        page.Append("<section class=\"runs-section\"><div class=\"section-title\"><h2>Recent runs</h2><span>")
            .Append(runs.Total.ToString("N0")).Append(" RUNS · EXPAND TO VIEW REPORTS</span></div>");
        if (runs.Total > 0)
        {
            page.Append("<div class=\"panel call-list\">");
            foreach (var run in runs.Runs) AppendRun(page, run);
            page.Append("</div>");
        }
        else
        {
            page.Append("<div class=\"panel runs-empty\">")
                .Append(runs.DatabaseExists ? "No runs in this date range. Try All time or check back later."
                    : "The schedule database or Runs table was not found. Check the writer’s location or set SCHEDULE_DB_PATH.")
                .Append("</div>");
        }
        page.Append("<a class=\"history-link\" href=\"").Append(H(RunsLink(leagueId, data.Period)))
            .Append("\">View all runs →</a></section>");
        if (data.Total == 0)
        {
            Empty(page, data.DatabaseExists);
            return Finish(page);
        }

        page.Append("<div class=\"columns\"><section class=\"league-section\"><div class=\"section-title\"><h2>Calls by tool</h2><span>EXPAND TO VIEW CALLS</span></div><div class=\"panel tool-list\">");
        var max = data.Tools.Max(t => t.Calls);
        foreach (var tool in data.Tools)
        {
            page.Append("<details class=\"tool-group\"><summary><div class=\"bar-label\"><span>")
                .Append(H(string.IsNullOrEmpty(tool.Name) ? "(unnamed)" : tool.Name))
                .Append("</span><strong>").Append(tool.Calls.ToString("N0"))
                .Append(" calls</strong>")
                .Append("</div><div class=\"track\"><div style=\"width:")
                .Append((100.0 * tool.Calls / max).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Append("%\"></div></div><span class=\"chevron\" aria-hidden=\"true\"></span></summary><div class=\"tool-entries\"><div class=\"table-wrap\"><table><thead><tr><th>RECORDED AT</th><th>PARAMETERS</th></tr></thead><tbody>");
            foreach (var call in tool.Entries)
                page.Append("<tr><td><time>").Append(H(FormatTime(call.Timestamp)))
                    .Append("</time></td><td><pre>")
                    .Append(H(string.IsNullOrWhiteSpace(call.Arguments) ? "(none)" : call.Arguments))
                    .Append("</pre></td></tr>");
            page.Append("</tbody></table></div></div></details>");
        }
        page.Append("</div></section><section class=\"league-section\"><div class=\"section-title\"><h2>Daily activity</h2><span>EXPAND FOR TOOL COUNTS</span></div><div class=\"panel day-list\">");
        var dayMax = data.Days.Max(d => d.Calls);
        foreach (var day in data.Days)
        {
            page.Append("<details class=\"day-group\"><summary class=\"day-row\"><time>").Append(H(day.Day))
                .Append("</time><div class=\"track\"><div style=\"width:")
                .Append((100.0 * day.Calls / dayMax).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Append("%\"></div></div><strong>").Append(day.Calls.ToString("N0"))
                .Append("</strong><span class=\"chevron\" aria-hidden=\"true\"></span></summary><ul class=\"day-breakdown\">");
            foreach (var tool in day.Tools)
                page.Append("<li><span>").Append(H(string.IsNullOrEmpty(tool.Name) ? "(unnamed)" : tool.Name))
                    .Append("</span><strong>").Append(tool.Calls.ToString("N0"))
                    .Append("</strong></li>");
            page.Append("</ul></details>");
        }
        page.Append("</div></section></div><a class=\"history-link\" href=\"").Append(H(HistoryLink(leagueId, data.Period)))
            .Append("\">View full call history →</a>");
        return Finish(page);
    }

    public static string History(string leagueId, string leagueName, HistoryData data)
    {
        var page = Start("Call history · " + leagueName, data.Period, leagueId, leagueName, history: true);
        if (data.Total == 0)
        {
            Empty(page, data.DatabaseExists);
            return Finish(page);
        }

        page.Append("<div class=\"section-title\"><h2>Call history</h2><span>")
            .Append(data.Total.ToString("N0")).Append(" CALLS · EXPAND TO VIEW ARGUMENTS</span></div><div class=\"panel call-list\">");
        foreach (var call in data.Calls)
        {
            page.Append("<details class=\"call\"><summary><span class=\"call-name\">").Append(H(call.Name))
                .Append("</span><time>").Append(H(FormatTime(call.Timestamp)))
                .Append("</time><span class=\"chevron\" aria-hidden=\"true\"></span></summary><div class=\"call-arguments\"><span>ARGUMENTS</span><pre>")
                .Append(H(string.IsNullOrWhiteSpace(call.Arguments) ? "(none)" : call.Arguments))
                .Append("</pre></div></details>");
        }
        page.Append("</div><nav class=\"pagination\" aria-label=\"Call history pages\">");
        if (data.Page > 1)
            page.Append("<a href=\"").Append(H(HistoryLink(leagueId, data.Period) + "&page=" + (data.Page - 1)))
                .Append("\">← Newer</a>");
        page.Append("<span>PAGE ").Append(data.Page.ToString("N0")).Append(" OF ")
            .Append(data.PageCount.ToString("N0")).Append("</span>");
        if (data.Page < data.PageCount)
            page.Append("<a href=\"").Append(H(HistoryLink(leagueId, data.Period) + "&page=" + (data.Page + 1)))
                .Append("\">Older →</a>");
        page.Append("</nav></section>");
        return Finish(page);
    }

    public static string Runs(string leagueId, string leagueName, RunsData data)
    {
        var page = Start("Runs · " + leagueName, data.Period, leagueId, leagueName, runs: true);
        if (data.Total == 0)
        {
            page.Append("<section class=\"empty\"><h2>No runs to show</h2><p>")
                .Append(data.DatabaseExists
                    ? "There are no scheduled runs in this date range. Try All time or check back later."
                    : "The schedule database or Runs table was not found. Check the writer’s location or set SCHEDULE_DB_PATH.")
                .Append("</p></section>");
            return Finish(page);
        }

        page.Append("<div class=\"section-title\"><h2>Scheduled runs</h2><span>")
            .Append(data.Total.ToString("N0")).Append(" RUNS · EXPAND TO VIEW REPORTS</span></div><div class=\"panel call-list\">");
        foreach (var run in data.Runs) AppendRun(page, run);
        page.Append("</div><nav class=\"pagination\" aria-label=\"Run pages\">");
        if (data.Page > 1)
            page.Append("<a href=\"").Append(H(RunsLink(leagueId, data.Period) + "&page=" + (data.Page - 1)))
                .Append("\">← Newer</a>");
        page.Append("<span>PAGE ").Append(data.Page.ToString("N0")).Append(" OF ")
            .Append(data.PageCount.ToString("N0")).Append("</span>");
        if (data.Page < data.PageCount)
            page.Append("<a href=\"").Append(H(RunsLink(leagueId, data.Period) + "&page=" + (data.Page + 1)))
                .Append("\">Older →</a>");
        page.Append("</nav>");
        return Finish(page);
    }

    private static void AppendRun(StringBuilder page, ScheduledRun run)
    {
        var status = !run.StatusAvailable ? "STATUS UNAVAILABLE" : run.ExecutedAt is not null ? "COMPLETED"
            : run.Body is not null ? "AWAITING DELIVERY" : "SCHEDULED";
        var title = !string.IsNullOrWhiteSpace(run.Subject) && !string.IsNullOrWhiteSpace(run.Body)
            ? run.Subject
            : string.IsNullOrWhiteSpace(run.Notes) ? "Scheduled check" : run.Notes;
        page.Append("<details class=\"call")
            .Append(!run.StatusAvailable || run.ExecutedAt is not null ? "" : run.Body is null ? " run-pending" : " run-undelivered")
            .Append("\"><summary><span class=\"call-name\">")
            .Append(H(title));
        if (run.StatusAvailable && run.ExecutedAt is null)
            page.Append(" <span class=\"run-status\">").Append(run.Body is null ? "NOT RUN YET" : "AWAITING DELIVERY")
                .Append("</span>");
        page
            .Append("</span><time>").Append(H(FormatTime(run.ScheduledAt)))
            .Append("</time><span class=\"chevron\" aria-hidden=\"true\"></span></summary><div class=\"call-arguments run-details\">")
            .Append("<span>").Append(status).Append("</span>");
        if (run.ExecutedAt is not null)
            page.Append("<p>Executed · ").Append(H(FormatTime(run.ExecutedAt))).Append("</p>");
        if (!string.IsNullOrWhiteSpace(run.Notes))
            page.Append("<p>Notes · ").Append(H(run.Notes)).Append("</p>");
        if (run.Subject is not null)
            page.Append("<p>Subject · ").Append(H(run.Subject)).Append("</p>");
        if (run.Body is not null)
            page.Append("<iframe title=\"Report: ").Append(H(title))
                .Append("\" sandbox=\"\" referrerpolicy=\"no-referrer\" srcdoc=\"")
                .Append(H(run.Body)).Append("\"></iframe>");
        else
            page.Append("<p>No report saved for this run.</p>");
        page.Append("</div></details>");
    }

    private static string FormatTime(string? value) =>
        value is { Length: >= 19 } ? value[..10] + " " + value.Substring(11, 8) : value ?? "—";

    private static string FormatRunTime(string? value, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(value)) return "—";
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return FormatTime(value);

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

    public static string Error()
    {
        var page = Start("Dashboard unavailable");
        page.Append("<section class=\"empty\"><h2>Could not load dashboard data</h2><p>Check the server logs and confirm STATS_DB_PATH and SCHEDULE_DB_PATH point to readable databases.</p></section>");
        return Finish(page);
    }
}
