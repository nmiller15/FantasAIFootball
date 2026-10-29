namespace FantasAIFootball.Utilities;

public static class Schedule
{
    public static bool IsTimeToRun(DateTime runDateTime)
    {
        var now = DateTime.Now;
        var truncatedRun = new DateTime(runDateTime.Year, runDateTime.Month, runDateTime.Day, runDateTime.Hour, 0, 0);
        var truncatedNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

        return truncatedRun <= truncatedNow;
    }
}
