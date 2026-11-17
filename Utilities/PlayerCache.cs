using FantasAIFootball.Models;
using FantasAIFootball.Models.League;
using FantasAIFootball.Utilities;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace FantasAIFootball;

public class PlayerCache
{
    private readonly string _dbName = "cache.db";
    private readonly string _connectionString;

    public PlayerCache()
    {
        var userAppData = UserData.GetAppDataLocation();
        _connectionString = $"Data Source={userAppData}/{_dbName}";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Players (
                Key TEXT PRIMARY KEY,
                Json TEXT,
                InsertedAt TEXT
            );
        ";

        command.ExecuteNonQuery();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS PlayerCatalog (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                UpdatedAt TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public async Task Cache(Dictionary<string, Player> players)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        var updatedAt = DateTime.Now.ToString("o");

        foreach (var kvp in players)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT OR REPLACE INTO Players (Key, Json, InsertedAt)
                VALUES (@Key, @Json, @InsertedAt);
            ";
            command.Parameters.AddWithValue("@Key", kvp.Key);
            command.Parameters.AddWithValue("@Json", Stringify.Model(kvp.Value));
            command.Parameters.AddWithValue("@InsertedAt", updatedAt);

            await command.ExecuteNonQueryAsync();
        }

        using var catalogCommand = connection.CreateCommand();
        catalogCommand.Transaction = transaction;
        catalogCommand.CommandText = "INSERT OR REPLACE INTO PlayerCatalog (Id, UpdatedAt) VALUES (1, @UpdatedAt)";
        catalogCommand.Parameters.AddWithValue("@UpdatedAt", updatedAt);
        await catalogCommand.ExecuteNonQueryAsync();
        transaction.Commit();
    }

    public async Task<CacheResult?> Get(string playerId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            SELECT 
                Key, 
                Json, 
                InsertedAt 
            FROM Players 
            WHERE Key = @Key;
        ";
        command.Parameters.AddWithValue("@Key", playerId);

        var reader = await command.ExecuteReaderAsync();

        while (reader.Read())
        {
            var key = reader.GetString(0);
            var json = reader.GetString(1);
            var insertedAt = reader.GetString(2);

            if (key == playerId)
            {
                return new CacheResult
                {
                    Key = key,
                    Json = json,
                    InsertedAt = DateTime.Parse(insertedAt)
                };
            }
        }

        return null;
    }

    public async Task<DateTime?> GetLastUpdated()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT UpdatedAt FROM PlayerCatalog WHERE Id = 1";
        var value = await command.ExecuteScalarAsync();
        return value is string timestamp ? DateTime.Parse(timestamp) : null;
    }

    public async Task<List<Player>> FindByName(string name)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Json FROM Players
            WHERE lower(trim(coalesce(json_extract(Json, '$.first_name'), '') || ' ' ||
                             coalesce(json_extract(Json, '$.last_name'), ''))) = lower(@name)
               OR lower(trim(json_extract(Json, '$.search_full_name'))) = lower(@name)
            """;
        command.Parameters.AddWithValue("@name", name.Trim());

        var matches = new List<Player>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var player = JsonSerializer.Deserialize<Player>(reader.GetString(0));
            if (player != null) matches.Add(player);
        }

        return matches;
    }
}
