using Microsoft.Data.Sqlite;

namespace StatsDashboard;

public record LeagueMemory(long Id, string UserId, string Content, string CreatedAt);
public record MemoriesData(IReadOnlyList<LeagueMemory> Entries, bool DatabaseExists);

public class MemoryStore(string databasePath)
{
    private readonly string _databasePath = Path.GetFullPath(databasePath);

    private SqliteConnection? Open(SqliteOpenMode mode)
    {
        // The writer owns this database and its schema. Never create it from the dashboard.
        if (!File.Exists(_databasePath)) return null;
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = mode,
            Pooling = false
        }.ToString());
        connection.Open();
        return connection;
    }

    private static bool HasMemoriesTable(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Memories'";
        return (long)command.ExecuteScalar()! > 0;
    }

    public IReadOnlyList<string> GetLeagueIds()
    {
        using var connection = Open(SqliteOpenMode.ReadOnly);
        if (connection is null || !HasMemoriesTable(connection)) return [];
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT LeagueId FROM Memories ORDER BY LeagueId";
        using var reader = command.ExecuteReader();
        var ids = new List<string>();
        while (reader.Read()) ids.Add(reader.GetString(0));
        return ids;
    }

    public MemoriesData GetMemories(string leagueId)
    {
        using var connection = Open(SqliteOpenMode.ReadOnly);
        if (connection is null || !HasMemoriesTable(connection)) return new([], false);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT rowid, UserId, Content, CreatedAt FROM Memories
            WHERE LeagueId = @league ORDER BY CreatedAt DESC, rowid DESC
            """;
        command.Parameters.AddWithValue("@league", leagueId);
        using var reader = command.ExecuteReader();
        var entries = new List<LeagueMemory>();
        while (reader.Read())
            entries.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        return new(entries, true);
    }

    public async Task AddMemoryAsync(string leagueId, string userId, string content)
    {
        using var connection = Open(SqliteOpenMode.ReadWrite)
            ?? throw new InvalidOperationException("The memory database was not found.");
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Memories (LeagueId, UserId, Content, CreatedAt)
            VALUES (@league, @user, @content, @createdAt)
            """;
        command.Parameters.AddWithValue("@league", leagueId);
        command.Parameters.AddWithValue("@user", userId);
        command.Parameters.AddWithValue("@content", content);
        command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("o"));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> UpdateMemoryAsync(string leagueId, long id, string content)
    {
        using var connection = Open(SqliteOpenMode.ReadWrite)
            ?? throw new InvalidOperationException("The memory database was not found.");
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Memories SET Content = @content WHERE rowid = @id AND LeagueId = @league";
        command.Parameters.AddWithValue("@content", content);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@league", leagueId);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteMemoryAsync(string leagueId, long id)
    {
        using var connection = Open(SqliteOpenMode.ReadWrite)
            ?? throw new InvalidOperationException("The memory database was not found.");
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Memories WHERE rowid = @id AND LeagueId = @league";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@league", leagueId);
        return await command.ExecuteNonQueryAsync() > 0;
    }
}
