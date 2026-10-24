using System.Net;
using System.Text;

namespace StatsDashboard;

public static class DashboardPage
{
    private static string H(string? value) => WebUtility.HtmlEncode(value ?? "");
    private static string Link(string id, Period period) => "/league/" + Uri.EscapeDataString(id) + "?period=" + period.Query;
    private static string HistoryLink(string id, Period period) => "/league/" + Uri.EscapeDataString(id) + "/history?period=" + period.Query;

    private static StringBuilder Start(string title, Period? period = null, string? leagueId = null,
        string? leagueName = null, bool history = false)
    {
        var page = new StringBuilder("""
            <!doctype html><html lang="en"><head><meta charset="utf-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <meta name="robots" content="noindex,nofollow">
            <title>Tool Call Stats</title><link rel="stylesheet" href="/style.css"><link rel="stylesheet" href="/calls.css"></head><body>
            <div class="shell"><header class="header"><a class="brand" href="/">FANTAS<span>AI</span>FOOTBALL <small>/ STATS</small></a>
            <span class="header-note">TOOL CALL MONITOR</span></header><main>
            """);
        if (leagueId is not null)
        {
            page.Append("<nav class=\"breadcrumb\"><a href=\"/?period=").Append(period?.Query)
                .Append("\">ALL LEAGUES</a> <span>/</span> ");
            if (history)
                page.Append("<a href=\"").Append(H(Link(leagueId, period!.Value))).Append("\">")
                    .Append(H(leagueName ?? leagueId)).Append("</a> <span>/</span> HISTORY");
            else
                page.Append(H(leagueName ?? leagueId));
            page.Append("</nav>");
        }

        page.Append("<div class=\"intro\"><div><p class=\"eyebrow\">LIVE FROM THE STATS DATABASE</p><h1>")
            .Append(H(title)).Append("</h1>");
        if (leagueId is not null)
            page.Append("<p class=\"headline-id\">LEAGUE ID · ").Append(H(leagueId)).Append("</p>");
        page.Append("<p class=\"subhead\">Tool activity, organized by league. Times are recorded in the writer’s local time.</p></div>");
        if (period is not null)
        {
            page.Append("<nav class=\"periods\" aria-label=\"Date range\">");
            foreach (var choice in new[] { Period.Parse("7"), Period.Parse("30"), Period.Parse("all") })
            {
                var url = leagueId is null ? "/?period=" + choice.Query
                    : history ? HistoryLink(leagueId, choice) : Link(leagueId, choice);
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

    public static string Overview(OverviewData data, IReadOnlyDictionary<string, string> names)
    {
        var page = Start("The league overview", data.Period);
        page.Append("<section class=\"metrics\"><div class=\"metric\"><span>TOTAL TOOL CALLS</span><strong>")
            .Append(data.Total.ToString("N0")).Append("</strong><small>").Append(H(data.Period.Label))
            .Append("</small></div><div class=\"metric\"><span>ACTIVE LEAGUES</span><strong>")
            .Append(data.Leagues.Count.ToString("N0"))
            .Append("</strong><small>With recorded calls</small></div></section>");

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
                    .Append("</strong><small>TOOLS USED</small></div></div><p>LAST CALL · ")
                    .Append(H(FormatTime(league.LastCall))).Append("</p></a>");
            }
            page.Append("</div>");
        }
        return Finish(page);
    }

    public static string League(string leagueId, string leagueName, LeagueData data)
    {
        var page = Start(string.IsNullOrWhiteSpace(leagueName) ? leagueId : leagueName, data.Period, leagueId, leagueName);
        page.Append("<section class=\"metrics\"><div class=\"metric\"><span>TOTAL TOOL CALLS</span><strong>")
            .Append(data.Total.ToString("N0")).Append("</strong><small>").Append(H(data.Period.Label))
            .Append("</small></div><div class=\"metric\"><span>TOOLS USED</span><strong>")
            .Append(data.Tools.Count.ToString("N0"))
            .Append("</strong><small>Distinct tool names</small></div></section>");
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

    private static string FormatTime(string? value) =>
        value is { Length: >= 19 } ? value[..10] + " " + value.Substring(11, 8) : value ?? "—";

    public static string Error()
    {
        var page = Start("Stats unavailable");
        page.Append("<section class=\"empty\"><h2>Could not read the stats database</h2><p>Check the server logs and confirm STATS_DB_PATH points to a readable stats.db with the ToolCallRecords table.</p></section>");
        return Finish(page);
    }
}
