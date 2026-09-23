using FantasAIFootball.Models.Memories;
using FantasAIFootball.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Repositories;

public class MemoryRepository
{
    private readonly string _dbName = "memory.db";
    private readonly string _connectionString;

    private readonly string _username;
    private readonly string _leagueId;

    public MemoryRepository(IConfiguration configuration)
    {
        _username = configuration.GetValue<string>("username") ?? throw new Exception("Username is not set. Please set the 'username' in your configuration.");
        _leagueId = configuration.GetValue<string>("leagueId") ?? throw new Exception("League ID is not set. Please set the 'leagueId' in your configuration.");

        var userAppData = UserData.GetAppDataLocation();
        _connectionString = $"Data Source={userAppData}/{_dbName}";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Memories (
                LeagueId TEXT,
                UserId TEXT,
                Content TEXT,
                CreatedAt TEXT
            );
        ";

        command.ExecuteNonQuery();
    }

    public async Task<List<Memory>> GetMemories()
    {
        using var connection = new SqliteConnection(_connectionString);

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            SELECT 
                LeagueId,
                UserId,
                Content,
                CreatedAt
            FROM Memories
            WHERE LeagueId = @LeagueId AND UserId = @Username
        ";

        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@Username", _username);

        var reader = await command.ExecuteReaderAsync();

        var memories = new List<Memory>();
        while (reader.Read())
        {
            var memory = new Memory
            {
                LeagueId = reader.GetString(0),
                UserId = reader.GetString(1),
                Content = reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3))
            };

            memories.Add(memory);
        }

        return memories;
    }

    public async Task AddMemory(string content)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            INSERT INTO Memories (LeagueId, UserId, Content, CreatedAt)
            VALUES (@LeagueId, @Username, @Content, @CreatedAt);
        ";

        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@Username", _username);
        command.Parameters.AddWithValue("@Content", content);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));

        await command.ExecuteNonQueryAsync();
    }
}
