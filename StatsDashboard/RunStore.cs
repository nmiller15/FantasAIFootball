using Microsoft.Data.Sqlite;

namespace StatsDashboard;

public record ScheduledRun(int Key, string ScheduledAt, string Notes, string? ExecutedAt,
    string? Subject, string? Body, bool StatusAvailable = true);
public record RunTiming(string? LastExecuted, string? NextScheduled);
public record RunReport(string LeagueId, int Key, string? Subject, string Body);

public record RunsData(Period Period, string LeagueId, long Total, IReadOnlyList<ScheduledRun> Runs,
    bool DatabaseExists, int Page, int PageSize = 20)
{
    public long PageCount => Math.Max(1, (Total + PageSize - 1) / PageSize);
}

public class RunStore(string databasePath)
{
    private readonly string _databasePath = Path.GetFullPath(databasePath);

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

    private static HashSet<string> Columns(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(Runs)";
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read()) columns.Add(reader.GetString(1));
        return columns;
    }

    public async Task ScheduleRunAsync(string leagueId, string notes)
    {
        // Never create a database here: the writer owns the schema and migrations.
        if (!File.Exists(_databasePath))
            throw new InvalidOperationException("The schedule database was not found.");

        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWrite,
            Pooling = false
        }.ToString());
        await connection.OpenAsync();

        var columns = Columns(connection);
        if (!new[] { "LeagueId", "RunDateTime", "Notes", "InsertedAt" }.All(columns.Contains))
            throw new InvalidOperationException("The schedule database does not have the expected Runs table.");

        var now = DateTime.Now;
        var thisHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Runs (LeagueId, RunDateTime, Notes, InsertedAt) VALUES (@league, @runAt, @notes, @insertedAt)";
        command.Parameters.AddWithValue("@league", leagueId);
        command.Parameters.AddWithValue("@runAt", thisHour.ToString("o"));
        command.Parameters.AddWithValue("@notes", notes);
        command.Parameters.AddWithValue("@insertedAt", now.ToString("o"));
        await command.ExecuteNonQueryAsync();
    }

    public RunReport? GetReport(string leagueId, int key)
    {
        using var connection = Open();
        if (connection is null) return null;
        var columns = Columns(connection);
        if (!columns.Contains("ReportBody") || !columns.Contains("ReportSubject")) return null;

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ReportSubject, ReportBody FROM Runs WHERE Key = @key AND LeagueId = @league";
        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@league", leagueId);
        using var reader = command.ExecuteReader();
        if (!reader.Read() || reader.IsDBNull(1)) return null;
        return new RunReport(leagueId, key, reader.IsDBNull(0) ? null : reader.GetString(0), reader.GetString(1));
    }

    public IReadOnlyList<string> GetLeagueIds(Period period)
    {
        using var connection = Open();
        if (connection is null || Columns(connection).Count == 0) return [];
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT LeagueId FROM Runs
            WHERE (@since = '' OR substr(RunDateTime, 1, 10) >= @since)
            ORDER BY LeagueId
            """;
        command.Parameters.AddWithValue("@since", period.Since?.ToString("yyyy-MM-dd") ?? "");
        using var reader = command.ExecuteReader();
        var ids = new List<string>();
        while (reader.Read()) ids.Add(reader.IsDBNull(0) ? "" : reader.GetString(0));
        return ids;
    }

    public IReadOnlyDictionary<string, RunTiming> GetRunTimings()
    {
        using var connection = Open();
        if (connection is null || !Columns(connection).Contains("ExecutedAt"))
            return new Dictionary<string, RunTiming>();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT LeagueId, MAX(ExecutedAt), MIN(CASE WHEN ExecutedAt IS NULL THEN RunDateTime END)
            FROM Runs GROUP BY LeagueId
            """;
        using var reader = command.ExecuteReader();
        var timings = new Dictionary<string, RunTiming>(StringComparer.Ordinal);
        while (reader.Read())
            timings[reader.IsDBNull(0) ? "" : reader.GetString(0)] = new(
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2));
        return timings;
    }

    public RunsData GetRuns(string leagueId, Period period, int requestedPage = 1, int pageSize = 20)
    {
        using var connection = Open();
        if (connection is null) return new(period, leagueId, 0, [], false, 1);
        var columns = Columns(connection);
        if (columns.Count == 0) return new(period, leagueId, 0, [], false, 1);

        using var count = connection.CreateCommand();
        count.CommandText = """
            SELECT COUNT(*) FROM Runs
            WHERE LeagueId = @league AND (@since = '' OR substr(RunDateTime, 1, 10) >= @since)
            """;
        count.Parameters.AddWithValue("@league", leagueId);
        count.Parameters.AddWithValue("@since", period.Since?.ToString("yyyy-MM-dd") ?? "");
        var total = (long)count.ExecuteScalar()!;
        var page = (int)Math.Clamp((long)requestedPage, 1, Math.Max(1, (total + pageSize - 1) / pageSize));

        using var command = connection.CreateCommand();
        var executed = columns.Contains("ExecutedAt") ? "ExecutedAt" : "NULL";
        var subject = columns.Contains("ReportSubject") ? "ReportSubject" : "NULL";
        var body = columns.Contains("ReportBody") ? "ReportBody" : "NULL";
        command.CommandText = $"""
            SELECT Key, RunDateTime, Notes, {executed}, {subject}, {body} FROM Runs
            WHERE LeagueId = @league AND (@since = '' OR substr(RunDateTime, 1, 10) >= @since)
            ORDER BY RunDateTime DESC, Key DESC LIMIT @limit OFFSET @offset
            """;
        command.Parameters.AddWithValue("@league", leagueId);
        command.Parameters.AddWithValue("@since", period.Since?.ToString("yyyy-MM-dd") ?? "");
        command.Parameters.AddWithValue("@limit", pageSize);
        command.Parameters.AddWithValue("@offset", (long)(page - 1) * pageSize);
        using var reader = command.ExecuteReader();
        var runs = new List<ScheduledRun>();
        while (reader.Read())
            runs.Add(new(reader.GetInt32(0), reader.IsDBNull(1) ? "" : reader.GetString(1),
                reader.IsDBNull(2) ? "" : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5),
                columns.Contains("ExecutedAt")));

        return new(period, leagueId, total, runs, true, page, pageSize);
    }
}
