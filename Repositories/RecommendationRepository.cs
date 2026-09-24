using FantasAIFootball.Models.Memories;
using FantasAIFootball.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Repositories;

public class RecommendationRepository
{
    private readonly string _dbName = "memory.db";
    private readonly string _connectionString;

    private readonly string _username;
    private readonly string _leagueId;

    public RecommendationRepository(IConfiguration configuration)
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
            CREATE TABLE IF NOT EXISTS Recommendations (
                LeagueId TEXT,
                UserId TEXT,
                PlayerName TEXT,
                Direction TEXT,
                Reason TEXT,
                Confidence REAL,
                CreatedAt TEXT
            );
        ";

        command.ExecuteNonQuery();
    }

    public async Task<List<Recommendation>> GetRecommendations()
    {
        using var connection = new SqliteConnection(_connectionString);

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            SELECT 
                LeagueId,
                UserId,
                PlayerName,
                Direction,
                Reason,
                Confidence,
                CreatedAt
            FROM Recommendations
            WHERE LeagueId = @LeagueId AND UserId = @Username
        ";

        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@Username", _username);

        var reader = await command.ExecuteReaderAsync();

        var recommendations = new List<Recommendation>();
        while (reader.Read())
        {
            recommendations.Add(new Recommendation
            {
                LeagueId = reader.GetString(0),
                UserId = reader.GetString(1),
                PlayerName = reader.GetString(2),
                Direction = reader.GetString(3),
                Reason = reader.GetString(4),
                Confidence = reader.GetDouble(5),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            });
        }

        return recommendations;
    }

    public async Task AddRecommendation(string playerName, string direction, string reason, double confidence)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
            INSERT INTO Recommendations (LeagueId, UserId, PlayerName, Direction, Reason, Confidence, CreatedAt)
            VALUES (@LeagueId, @Username, @PlayerName, @Direction, @Reason, @Confidence, @CreatedAt)
        ";

        command.Parameters.AddWithValue("@LeagueId", _leagueId);
        command.Parameters.AddWithValue("@Username", _username);
        command.Parameters.AddWithValue("@PlayerName", playerName);
        command.Parameters.AddWithValue("@Direction", direction);
        command.Parameters.AddWithValue("@Reason", reason);
        command.Parameters.AddWithValue("@Confidence", confidence);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

        await command.ExecuteNonQueryAsync();
    }
}
