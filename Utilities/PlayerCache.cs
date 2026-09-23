using FantasAIFootball.Models;
using FantasAIFootball.Models.League;
using FantasAIFootball.Utilities;
using Microsoft.Data.Sqlite;

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
    }

    public async Task Cache(Dictionary<string, Player> players)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        foreach (var kvp in players)
        {
            using var command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = @"
                INSERT OR REPLACE INTO Players (Key, Json, InsertedAt)
                VALUES (@Key, @Json, @InsertedAt);
            ";
            command.Parameters.AddWithValue("@Key", kvp.Key);
            command.Parameters.AddWithValue("@Json", Stringify.Model(kvp.Value));
            command.Parameters.AddWithValue("@InsertedAt", DateTime.Now.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }
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
}
