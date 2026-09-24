using System.Text.Json;
using FantasAIFootball.Models.Interactions;
using FantasAIFootball.Repositories;

namespace FantasAIFootball.Services;

public class FunctionService
{
    private readonly LeagueRepository _leagueRepository;
    private readonly WebSearchRepository _webSearchRepository;
    private readonly MemoryRepository _memoryRepository;
    private readonly RecommendationRepository _recommendationRepository;

    public FunctionService(LeagueRepository leagueRepository, WebSearchRepository webSearchRepository, MemoryRepository memoryRepository, RecommendationRepository recommendationRepository)
    {
        _leagueRepository = leagueRepository;
        _webSearchRepository = webSearchRepository;
        _memoryRepository = memoryRepository;
        _recommendationRepository = recommendationRepository;
    }

    public async Task<FunctionResultStep> GetFunctionResultStep(FunctionCallStep call)
    {
        Log(call);
        try
        {
            switch (call.Name)
            {
                case "GetLeague":
                    var league = await _leagueRepository.GetLeague();
                    return league == null
                        ? FunctionResultStep.Failure(call, "League not found, unable to proceed.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(league));

                case "GetScoringSettings":
                    var scoringSettings = await _leagueRepository.GetScoringSettings();
                    return scoringSettings == null
                        ? FunctionResultStep.Failure(call, "Scoring settings not found, unable to proceed.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(scoringSettings));

                case "GetRosters":
                    var rosters = await _leagueRepository.GetRosters();
                    return rosters == null
                        ? FunctionResultStep.Failure(call, "Rosters not found, unable to proceed.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(rosters));

                case "GetMyRoster":
                    var myRoster = await _leagueRepository.GetMyRoster();
                    return myRoster == null
                        ? FunctionResultStep.Failure(call, "My roster not found, unable to proceed.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(myRoster));

                case "GetUserRoster":
                    if (!call.Arguments.TryGetProperty("userId", out var userIdElement) || userIdElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'userId' argument.");
                    }
                    var userId = userIdElement.GetString();
                    var userRoster = await _leagueRepository.GetUserRoster(userId);
                    return userRoster == null
                        ? FunctionResultStep.Failure(call, $"Roster for user '{userId}' not found.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(userRoster));

                case "GetUser":
                    if (!call.Arguments.TryGetProperty("username", out var usernameElement) || usernameElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'username' argument.");
                    }
                    var username = usernameElement.GetString();
                    var user = await _leagueRepository.GetUser(username);
                    return user == null
                        ? FunctionResultStep.Failure(call, $"User '{username}' not found.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(user));

                case "GetPlayer":
                    if (!call.Arguments.TryGetProperty("playerId", out var playerIdElement) || playerIdElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'playerId' argument.");
                    }
                    var playerId = playerIdElement.GetString();
                    var player = await _leagueRepository.GetPlayer(playerId);
                    return player == null
                        ? FunctionResultStep.Failure(call, $"Player '{playerId}' not found.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(player));

                case "GetNflState":
                    var nflState = await _leagueRepository.GetNflState();
                    return nflState == null
                        ? FunctionResultStep.Failure(call, "NFL state not found, unable to proceed.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(nflState));

                case "GetMatchups":
                    if (!call.Arguments.TryGetProperty("week", out var weekElement) || weekElement.ValueKind != System.Text.Json.JsonValueKind.Number)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'week' argument.");
                    }
                    var matchups = await _leagueRepository.GetMatchups(weekElement.GetInt32());
                    return !matchups.Any()
                        ? FunctionResultStep.Failure(call, $"No matchups found for week {weekElement.GetInt32()}.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(matchups));

                case "GetMatchupByRosterId":
                    if (!call.Arguments.TryGetProperty("rosterId", out var rosterIdElement) || rosterIdElement.ValueKind != System.Text.Json.JsonValueKind.Number)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'rosterId' argument.");
                    }
                    if (!call.Arguments.TryGetProperty("week", out var matchupWeekElement) || matchupWeekElement.ValueKind != System.Text.Json.JsonValueKind.Number)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'rosterId' argument.");
                    }
                    var matchup = await _leagueRepository.GetMatchupByRosterId(matchupWeekElement.GetInt32(), rosterIdElement.GetInt32());
                    return matchup == null
                        ? FunctionResultStep.Failure(call, $"No matchup found for rosterId {rosterIdElement.GetInt32()} in week {matchupWeekElement.GetInt32()}.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(matchup));

                case "GetCoorespondingMatchup":
                    if (!call.Arguments.TryGetProperty("matchupId", out var matchupIdElement) || matchupIdElement.ValueKind != System.Text.Json.JsonValueKind.Number)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'matchupId' argument.");
                    }
                    if (!call.Arguments.TryGetProperty("week", out var correspondingWeekElement) || correspondingWeekElement.ValueKind != System.Text.Json.JsonValueKind.Number)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'week' argument.");
                    }
                    var correspondingMatchup = await _leagueRepository.GetCoorespondingMatchup(correspondingWeekElement.GetInt32(), matchupIdElement.GetInt32());
                    return correspondingMatchup == null
                        ? FunctionResultStep.Failure(call, $"No corresponding matchup found for matchupId {matchupIdElement.GetInt32()} in week {correspondingWeekElement.GetInt32()}.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(correspondingMatchup));

                case "WebSearch":
                    if (!call.Arguments.TryGetProperty("query", out var queryElement) || queryElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'query' argument.");
                    }
                    var startDate = call.Arguments.TryGetProperty("startDate", out var startDateJson) && startDateJson.ValueKind == System.Text.Json.JsonValueKind.String
                        ? DateTime.Parse(startDateJson.GetString())
                        : (DateTime?)null;
                    var endDate = call.Arguments.TryGetProperty("endDate", out var endDateJson) && endDateJson.ValueKind == System.Text.Json.JsonValueKind.String
                        ? DateTime.Parse(endDateJson.GetString())
                        : (DateTime?)null;

                    var query = queryElement.GetString();
                    var searchResult = await _webSearchRepository.Query(query, startDate, endDate);
                    return searchResult == null
                        ? FunctionResultStep.Failure(call, $"No search results found for query '{query}'.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(searchResult));

                case "ScrapeUrls":
                    if (!call.Arguments.TryGetProperty("url", out var urlElement) || urlElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'url' argument.");
                    }

                    var url = urlElement.GetString();
                    if (string.IsNullOrEmpty(url))
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'url' argument.");
                    }

                    var scrapeResult = await _webSearchRepository.Extract(new List<string> { url });
                    return scrapeResult == null
                        ? FunctionResultStep.Failure(call, "No scrape results found for the provided URLs.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(scrapeResult));

                case "GetMemories":
                    var memories = await _memoryRepository.GetMemories();
                    return memories == null
                        ? FunctionResultStep.Failure(call, "Could not retrieve memories due to an internal error.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(memories));

                case "AddMemory":
                    if (!call.Arguments.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'content' argument.");
                    }

                    await _memoryRepository.AddMemory(contentElement.GetString());
                    return FunctionResultStep.Success(call, "Memory added successfully.");

                case "GetRecommendations":
                    var recommendations = await _recommendationRepository.GetRecommendations();
                    return recommendations == null
                        ? FunctionResultStep.Failure(call, "Could not retrieve recommendations due to an internal error.")
                        : FunctionResultStep.Success(call, System.Text.Json.JsonSerializer.Serialize(recommendations));

                case "AddRecommendation":
                    if (!call.Arguments.TryGetProperty("playerName", out var reccPlayerNameElement) || reccPlayerNameElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'playerName' argument.");
                    }
                    if (!call.Arguments.TryGetProperty("direction", out var directionElement) || directionElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'direction' argument.");
                    }
                    if (!call.Arguments.TryGetProperty("reason", out var reasonElement) || reasonElement.ValueKind != System.Text.Json.JsonValueKind.String)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'reason' argument.");
                    }
                    if (!call.Arguments.TryGetProperty("confidence", out var confidenceElement) || confidenceElement.ValueKind != System.Text.Json.JsonValueKind.Number ||
                            confidenceElement.GetDouble() < 0 || confidenceElement.GetDouble() > 1)
                    {
                        return FunctionResultStep.Failure(call, "Missing or invalid 'confidence' argument.");
                    }

                    await _recommendationRepository.AddRecommendation(
                        reccPlayerNameElement.GetString(),
                        directionElement.GetString(),
                        reasonElement.GetString(),
                        confidenceElement.GetDouble()
                    );

                    return FunctionResultStep.Success(call, "Recommendation added successfully.");

                default:
                    return FunctionResultStep.Failure(call, $"Function '{call.Name}' does not exist.");
            }
        }
        catch (Exception ex)
        {
            return FunctionResultStep.Failure(call, $"Error executing function '{call.Name}': {ex.Message}");
        }
    }

    private void Log(FunctionCallStep call)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));
        Console.Write("Function call: ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(call.Name);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("(");
        if (call.Arguments.GetPropertyCount() > 0)
        {
            var first = true;
            foreach (var prop in call.Arguments.EnumerateObject())
            {
                if (!first)
                {
                    Console.Write(", ");

                }
                Console.Write($"{prop.Name}: {prop.Value}");
                first = false;
            }
        }
        Console.Write(")");
        Console.WriteLine();
        Console.ResetColor();
    }
}
