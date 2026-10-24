using Microsoft.Data.Sqlite;

namespace StatsDashboard;

public readonly record struct Period(string Label, string Query, DateTime? Since)
{
    public static Period Parse(string? value) => value switch
    {
        "7" => new("Last 7 days", "7", DateTime.Today.AddDays(-6)),
        "all" => new("All time", "all", null),
        _ => new("Last 30 days", "30", DateTime.Today.AddDays(-29))
    };
}

public record LeagueSummary(string Id, long Calls, long Tools, string? LastCall);
public record ToolCall(string Timestamp, string Arguments);
public record ToolSummary(string Name, long Calls, IReadOnlyList<ToolCall> Entries);
public record DayToolSummary(string Name, long Calls);
public record DaySummary(string Day, long Calls, IReadOnlyList<DayToolSummary> Tools);
public record RecentCall(string Name, string Timestamp, string Arguments);
public record OverviewData(Period Period, long Total, IReadOnlyList<LeagueSummary> Leagues, bool DatabaseExists);
public record LeagueData(Period Period, string Id, long Total, IReadOnlyList<ToolSummary> Tools,
    IReadOnlyList<DaySummary> Days, bool DatabaseExists);
public record HistoryData(Period Period, string Id, long Total, IReadOnlyList<RecentCall> Calls, bool DatabaseExists, int Page)
{
    public long PageCount => Math.Max(1, (Total + 19) / 20);
}

public class StatsStore(string databasePath)
{
    private readonly string _databasePath = Path.GetFullPath(databasePath);

    // The writer owns the schema. A missing file is not created by the dashboard.
    private SqliteConnection? Open()
    {
        if (!File.Exists(_databasePath)) return null;
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString());
        connection.Open();
        return connection;
    }

    private static SqliteCommand Query(SqliteConnection connection, string sql, Period period, string? leagueId = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        // Existing records store DateTime.Now.ToString("o"). Compare only the local calendar date
        // so records with different offset suffixes retain the writer's day grouping.
        command.Parameters.AddWithValue("@since", period.Since?.ToString("yyyy-MM-dd") ?? "");
        if (leagueId is not null) command.Parameters.AddWithValue("@league", leagueId);
        return command;
    }

    public OverviewData GetOverview(Period period)
    {
        using var connection = Open();
        if (connection is null) return new(period, 0, [], false);

        using var command = Query(connection, """
            SELECT LeagueId, COUNT(*), COUNT(DISTINCT Name), MAX(InsertedAt)
            FROM ToolCallRecords
            WHERE (@since = '' OR substr(InsertedAt, 1, 10) >= @since)
            GROUP BY LeagueId
            ORDER BY COUNT(*) DESC, LeagueId
            """, period);
        using var reader = command.ExecuteReader();
        var leagues = new List<LeagueSummary>();
        while (reader.Read())
        {
            leagues.Add(new(reader.IsDBNull(0) ? "" : reader.GetString(0), reader.GetInt64(1),
                reader.GetInt64(2), reader.IsDBNull(3) ? null : reader.GetString(3)));
        }
        return new(period, leagues.Sum(l => l.Calls), leagues, true);
    }

    public LeagueData GetLeague(string leagueId, Period period)
    {
        using var connection = Open();
        if (connection is null) return new(period, leagueId, 0, [], [], false);

        var counts = new List<(string Name, long Count)>();
        using (var command = Query(connection, """
            SELECT Name, COUNT(*) FROM ToolCallRecords
            WHERE LeagueId = @league AND (@since = '' OR substr(InsertedAt, 1, 10) >= @since)
            GROUP BY Name ORDER BY COUNT(*) DESC, Name
            """, period, leagueId))
        using (var reader = command.ExecuteReader())
            while (reader.Read()) counts.Add((reader.IsDBNull(0) ? "" : reader.GetString(0), reader.GetInt64(1)));

        var entries = new Dictionary<string, List<ToolCall>>();
        var dayTools = new Dictionary<string, Dictionary<string, long>>();
        using (var command = Query(connection, """
            SELECT Name, InsertedAt, Parameters FROM ToolCallRecords
            WHERE LeagueId = @league AND (@since = '' OR substr(InsertedAt, 1, 10) >= @since)
            ORDER BY InsertedAt DESC, Key DESC
            """, period, leagueId))
        using (var reader = command.ExecuteReader())
            while (reader.Read())
            {
                var name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                var timestamp = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var arguments = reader.IsDBNull(2) ? "" : reader.GetString(2);
                if (!entries.TryGetValue(name, out var calls)) entries[name] = calls = [];
                calls.Add(new(timestamp, arguments));
                var day = timestamp.Length > 10 ? timestamp[..10] : timestamp;
                if (!dayTools.TryGetValue(day, out var dailyCounts)) dayTools[day] = dailyCounts = [];
                dailyCounts[name] = dailyCounts.GetValueOrDefault(name) + 1;
            }

        var tools = counts.Select(count => new ToolSummary(count.Name, count.Count, entries[count.Name])).ToList();

        var days = new List<DaySummary>();
        using (var command = Query(connection, """
            SELECT substr(InsertedAt, 1, 10), COUNT(*) FROM ToolCallRecords
            WHERE LeagueId = @league AND (@since = '' OR substr(InsertedAt, 1, 10) >= @since)
            GROUP BY substr(InsertedAt, 1, 10) ORDER BY substr(InsertedAt, 1, 10) DESC LIMIT 30
            """, period, leagueId))
        using (var reader = command.ExecuteReader())
            while (reader.Read())
            {
                var day = reader.IsDBNull(0) ? "" : reader.GetString(0);
                var breakdown = dayTools[day]
                    .OrderByDescending(tool => tool.Value).ThenBy(tool => tool.Key, StringComparer.Ordinal)
                    .Select(tool => new DayToolSummary(tool.Key, tool.Value)).ToList();
                days.Add(new(day, reader.GetInt64(1), breakdown));
            }

        return new(period, leagueId, tools.Sum(t => t.Calls), tools, days, true);
    }

    public HistoryData GetHistory(string leagueId, Period period, int requestedPage = 1)
    {
        using var connection = Open();
        if (connection is null) return new(period, leagueId, 0, [], false, 1);

        long total;
        using (var command = Query(connection, """
            SELECT COUNT(*) FROM ToolCallRecords
            WHERE LeagueId = @league AND (@since = '' OR substr(InsertedAt, 1, 10) >= @since)
            """, period, leagueId))
            total = (long)command.ExecuteScalar()!;

        var page = (int)Math.Clamp((long)requestedPage, 1, Math.Max(1, (total + 19) / 20));
        var recent = new List<RecentCall>();
        using (var command = Query(connection, """
            SELECT Name, InsertedAt, Parameters FROM ToolCallRecords
            WHERE LeagueId = @league AND (@since = '' OR substr(InsertedAt, 1, 10) >= @since)
            ORDER BY InsertedAt DESC, Key DESC LIMIT 20 OFFSET @offset
            """, period, leagueId))
        {
            command.Parameters.AddWithValue("@offset", (long)(page - 1) * 20);
            using var reader = command.ExecuteReader();
            while (reader.Read()) recent.Add(new(reader.IsDBNull(0) ? "" : reader.GetString(0),
                reader.IsDBNull(1) ? "" : reader.GetString(1), reader.IsDBNull(2) ? "" : reader.GetString(2)));
        }

        return new(period, leagueId, total, recent, true, page);
    }
}
