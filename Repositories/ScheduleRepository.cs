using System.Globalization;
using FantasAIFootball.Models.Schedule;
using FantasAIFootball.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball;

public class ScheduleRepository
{
    private readonly string _leagueId;
    private readonly string _dbName = "schedule.db";
    private readonly string _connectionString;

    public ScheduleRepository(IConfiguration configuration)
    {
        _leagueId = configuration.GetValue<string>("leagueId") ?? throw new Exception("League ID is not set. Please set the 'leagueId' in your configuration.");

        var userAppData = UserData.GetAppDataLocation();
        _connectionString = $"Data Source={userAppData}/{_dbName}";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Runs (
                Key INTEGER PRIMARY KEY AUTOINCREMENT,
                LeagueId TEXT,
                RunDateTime TEXT,
                Notes TEXT,
                InsertedAt TEXT,
                ExecutedAt TEXT,
                ReportSubject TEXT,
                ReportBody TEXT
            );
        ";

        command.ExecuteNonQuery();

        // Existing installations may already have the Runs table without these columns.
        foreach (var column in new[] { "ExecutedAt", "ReportSubject", "ReportBody" })
        {
            command.CommandText = "PRAGMA table_info(Runs);";
            var exists = false;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    exists |= reader.GetString(1) == column;
                }
            }

            if (!exists)
            {
                command.CommandText = $"ALTER TABLE Runs ADD COLUMN {column} TEXT;";
                command.ExecuteNonQuery();
            }
        }
    }

    public async Task<Run?> GetNextRun()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Key, LeagueId, RunDateTime, Notes, InsertedAt, ExecutedAt, ReportSubject, ReportBody
            FROM Runs
            WHERE LeagueId = @LeagueId AND ExecutedAt IS NULL AND datetime(RunDateTime) > datetime(@CurrentDateTime)
            ORDER BY datetime(RunDateTime) ASC
            LIMIT 1;
        ";

        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@CurrentDateTime", DateTime.Now.ToString("o"));

        var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadRun(reader);
        }

        return null;
    }

    public async Task<Run?> GetDueRun()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Key, LeagueId, RunDateTime, Notes, InsertedAt, ExecutedAt, ReportSubject, ReportBody
            FROM Runs
            WHERE LeagueId = @LeagueId AND ExecutedAt IS NULL AND datetime(RunDateTime) <= datetime(@CurrentDateTime)
            ORDER BY datetime(RunDateTime) ASC
            LIMIT 1;
        ";
        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@CurrentDateTime", DateTime.Now.ToString("o"));

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadRun(reader) : null;
    }

    public async Task MarkRunExecuted(int key)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Runs SET ExecutedAt = @ExecutedAt WHERE Key = @Key AND LeagueId = @LeagueId AND ExecutedAt IS NULL;";
        command.Parameters.AddWithValue("@ExecutedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        if (await command.ExecuteNonQueryAsync() != 1)
        {
            throw new InvalidOperationException("Scheduled run could not be marked executed.");
        }
    }

    public async Task<(string Subject, string Body)?> GetPendingReport(int key)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ReportSubject, ReportBody FROM Runs WHERE Key = @Key AND LeagueId = @LeagueId AND ExecutedAt IS NULL;";
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() && !reader.IsDBNull(0) && !reader.IsDBNull(1)
            ? (reader.GetString(0), reader.GetString(1))
            : null;
    }

    public async Task SaveReport(int key, string subject, string body)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Runs SET ReportSubject = @Subject, ReportBody = @Body WHERE Key = @Key AND LeagueId = @LeagueId AND ExecutedAt IS NULL;";
        command.Parameters.AddWithValue("@Subject", subject);
        command.Parameters.AddWithValue("@Body", body);
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        if (await command.ExecuteNonQueryAsync() != 1)
        {
            throw new InvalidOperationException("Could not save report for delivery retry.");
        }
    }

    public async Task ScheduleRun(DateTime runDateTime, string notes)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            INSERT INTO Runs (LeagueId, RunDateTime, Notes, InsertedAt) 
            VALUES (@LeagueId, @RunDateTime, @Notes, @InsertedAt);
        ";

        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@RunDateTime", runDateTime.ToString("o"));
        command.Parameters.AddWithValue("@Notes", notes);
        command.Parameters.AddWithValue("@InsertedAt", DateTime.Now.ToString("o"));

        await command.ExecuteNonQueryAsync();
    }

    private static Run ReadRun(SqliteDataReader reader) => new()
    {
        Key = reader.GetInt32(0),
        LeagueId = reader.GetString(1),
        RunDateTime = DateTime.Parse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
        Notes = reader.IsDBNull(3) ? "" : reader.GetString(3),
        InsertedAt = DateTime.Parse(reader.GetString(4)),
        ExecutedAt = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
        ReportSubject = reader.IsDBNull(6) ? null : reader.GetString(6),
        ReportBody = reader.IsDBNull(7) ? null : reader.GetString(7)
    };
}
