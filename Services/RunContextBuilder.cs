using FantasAIFootball.Models.League;

namespace FantasAIFootball.Services;

/// <summary>
/// Builds the runtime context block that is prepended to the task prompt. The model
/// must never infer the current date, league week, waiver deadline, or trade
/// deadline on its own, so all of it is resolved here and passed in explicitly.
/// </summary>
public static class RunContextBuilder
{
    private const int FreeAgentWaiverType = 0;

    private static readonly string[] DayNames =
    [
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ];

    public static string Build(
        DateTimeOffset now,
        NflState nflState,
        LeagueSettings? settings,
        int? waiverDayOverride = null)
    {
        var eastern = ToEastern(now);

        var effective = settings;
        if (waiverDayOverride is >= 0 and <= 6 && settings != null)
        {
            effective = new LeagueSettings
            {
                WaiverType = settings.WaiverType,
                WaiverDayOfWeek = waiverDayOverride,
                WaiverBudget = settings.WaiverBudget,
                WaiverClearDays = settings.WaiverClearDays,
                WaiverBidMin = settings.WaiverBidMin,
                DailyWaivers = settings.DailyWaivers,
                DailyWaiversHour = settings.DailyWaiversHour,
                TradeDeadline = settings.TradeDeadline,
                DisableTrades = settings.DisableTrades,
                PlayoffWeekStart = settings.PlayoffWeekStart,
                PlayoffTeams = settings.PlayoffTeams
            };
        }

        var lines = new List<string>
        {
            $"Today is {DayNames[(int)eastern.DayOfWeek]}, {eastern:yyyy-MM-dd}.",
            $"NFL state: season {nflState.Season}, season_type '{nflState.SeasonType}', week {nflState.Week}, display_week {nflState.DisplayWeek}."
        };

        lines.AddRange(BuildWaiverLines(eastern, effective));

        var tradeLine = BuildTradeLine(nflState, effective);
        if (!string.IsNullOrWhiteSpace(tradeLine))
        {
            lines.Add(tradeLine);
        }

        var playoffLine = BuildPlayoffLine(nflState, effective);
        if (!string.IsNullOrWhiteSpace(playoffLine))
        {
            lines.Add(playoffLine);
        }

        return string.Join("\n", lines);
    }

    private static IEnumerable<string> BuildWaiverLines(
        DateTimeOffset eastern,
        LeagueSettings? settings)
    {
        if (settings?.WaiverType == null)
        {
            yield return "Waiver mechanics: not reported by the league API. Assume a weekly waiver deadline on Tuesday night.";
            yield return "Next waiver deadline: unknown.";
            yield break;
        }

        if (settings.WaiverType == FreeAgentWaiverType)
        {
            yield return "Waiver mechanics: free agent (rolling). Players become available immediately and are not protected by waiver priority, so there is no weekly deadline.";
            yield break;
        }

        var budget = settings.WaiverBudget ?? 0;
        yield return budget > 0
            ? $"Waiver mechanics: waiver priority (weekly), with a FAAB budget of {budget}."
            : "Waiver mechanics: waiver priority (weekly). FAAB is not in use.";

        if (settings.DailyWaivers is > 0)
        {
            yield return $"Waiver mechanics: waivers process daily at {settings.DailyWaiversHour ?? 0:00} ET.";
        }

        var day = settings.WaiverDayOfWeek;
        if (day == null || day is < 0 or > 6)
        {
            yield return "Next waiver deadline: not reported by the league API.";
            yield break;
        }

        // Sleeper counts waiver_day_of_week from Sunday (0 = Sunday). For weekly
        // priority waivers the period clears on that day, and stays open for
        // waiver_clear_days after the final game of the week, so there is no hour
        // to report. The resolved day name is printed with the raw values so a
        // misconfiguration is visible rather than silent.
        var offset = (day.Value - (int)eastern.DayOfWeek + 7) % 7;
        var clearsOn = eastern.Date.AddDays(offset);

        var timing = settings.DailyWaivers is > 0
            ? $"processes daily at {settings.DailyWaiversHour ?? 0:00} ET"
            : $"clears after the final game of the week plus {settings.WaiverClearDays ?? 0} day{(settings.WaiverClearDays == 1 ? "" : "s")}";

        var relative = offset switch
        {
            0 => "today",
            1 => "tomorrow",
            _ => $"in {offset} days"
        };

        yield return $"Next waiver deadline: the period {timing}, landing on {DayNames[day.Value]} {clearsOn:MMM d} ({relative}). " +
                     $"[league reports waiver_type={settings.WaiverType}, waiver_day_of_week={day.Value}, waiver_clear_days={settings.WaiverClearDays ?? 0}]";

        if (settings.WaiverBidMin is > 0)
        {
            yield return $"Minimum waiver bid is {settings.WaiverBidMin} FAAB.";
        }
    }

    private static string BuildTradeLine(NflState nflState, LeagueSettings? settings)
    {
        if (settings == null)
        {
            return string.Empty;
        }

        if (settings.DisableTrades is > 0)
        {
            return "Trading is disabled in this league. Do not propose or evaluate trades.";
        }

        if (settings.TradeDeadline == null)
        {
            return string.Empty;
        }

        var deadline = settings.TradeDeadline.Value;

        if (nflState.SeasonType != "regular" || nflState.Week > deadline)
        {
            return $"The trade deadline has passed (it fell after week {deadline}). Do not propose trades.";
        }

        var remaining = deadline - nflState.Week;
        return remaining == 0
            ? $"This is the final week for trading; the trade deadline falls after week {deadline}."
            : $"Trading closes after week {deadline} ({remaining} week{(remaining == 1 ? "" : "s")} from now).";
    }

    private static string BuildPlayoffLine(NflState nflState, LeagueSettings? settings)
    {
        if (settings?.PlayoffWeekStart == null)
        {
            return string.Empty;
        }

        var start = settings.PlayoffWeekStart.Value;

        if (!nflState.SeasonType.Equals("regular", StringComparison.OrdinalIgnoreCase))
        {
            return $"Playoffs are underway (playoff_week_start was {start}, {settings.PlayoffTeams} teams advance).";
        }

        var weeksRemaining = start - nflState.Week;

        return weeksRemaining > 0
            ? $"Playoffs start in week {start} ({weeksRemaining} week{(weeksRemaining == 1 ? "" : "s")} from now); {settings.PlayoffTeams} teams advance."
            : $"Playoffs were scheduled to start in week {start}.";
    }

    private static DateTimeOffset ToEastern(DateTimeOffset value)
    {
        foreach (var id in new[] { "America/New_York", "Eastern Standard Time" })
        {
            try
            {
                return TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById(id));
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                continue;
            }
        }

        return value;
    }
}