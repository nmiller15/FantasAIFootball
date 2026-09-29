using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FantasAIFootball.Models.League;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FantasAIFootball.Repositories;

public class LeagueRepository
{
    private readonly HttpClient _httpClient;
    private readonly PlayerCache _playerCache;

    private readonly string _username;
    private readonly string _leagueId;

    private string? _userId;
    private string UserId
    {
        get
        {
            if (_userId == null)
            {
                Log.Information($"Fetching user ID for username {_username}");
                var user = GetUser(_username).Result ?? throw new Exception($"User {_username} not found");
                _userId = user.UserId;
            }
            return _userId;
        }
    }

    public LeagueRepository(IConfiguration configuration, PlayerCache playerCache)
    {
        _username = configuration.GetValue<string>("username") ?? throw new Exception("Username is not set. Please set the 'username' in your configuration.");
        _leagueId = configuration.GetValue<string>("leagueId") ?? throw new Exception("League ID is not set. Please set the 'leagueId' in your configuration.");

        _playerCache = playerCache;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.sleeper.app/v1/")
        };
    }

    private async Task<HttpResponseMessage> Get(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        Log.Debug($"get {_httpClient.BaseAddress}{request.RequestUri}");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Log.Information("League: get {Path} - {StatusCode}", path, response.StatusCode);
            }
            else
            {
                Log.Error("League: get {Path} - {StatusCode}", path, response.StatusCode);
            }

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
            {
                response.EnsureSuccessStatusCode();
                return response;
            }

            var delay = response.Headers?.RetryAfter?.Delta
                ?? TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt));

            response.Dispose();

            Log.Warning("Rate limited. Retrying in {Delay} seconds...", delay.TotalSeconds);
            await Task.Delay(delay);
        }

        throw new UnreachableException();
    }

    public async Task<League?> GetLeague()
    {
        var response = await Get($"league/{_leagueId}");

        var league = JsonSerializer.Deserialize<League>(await response.Content.ReadAsStreamAsync());
        return league;
    }

    public async Task<ScoringSettings?> GetScoringSettings()
    {
        var league = await GetLeague();
        return league?.ScoringSettings;
    }

    public async Task<List<Roster>> GetRosters()
    {
        var response = await Get($"league/{_leagueId}/rosters");

        var rosters = JsonSerializer.Deserialize<List<Roster>>(await response.Content.ReadAsStreamAsync());
        return rosters ?? [];
    }

    public async Task<Roster?> GetMyRoster()
    {
        return await GetUserRoster(UserId);
    }

    public async Task<Roster?> GetUserRoster(string userId)
    {
        var rosters = await GetRosters();

        return rosters.FirstOrDefault(r => r.OwnerId == userId);
    }

    public async Task<User?> GetUser(string username)
    {
        var response = await Get($"user/{username}");

        var user = JsonSerializer.Deserialize<User>(await response.Content.ReadAsStreamAsync());
        return user;
    }

    public async Task<Player> GetPlayer(string playerId)
    {
        var cacheResult = await _playerCache.Get(playerId);
        if (cacheResult != null && cacheResult.InsertedAt.Day > DateTime.Today.AddDays(-1).Day)
        {
            return JsonSerializer.Deserialize<Player>(cacheResult.Json);
        }

        Log.Warning("Cache missed, last fetched at {InsertedAt} Fetching from API...", cacheResult?.InsertedAt);

        var response = await Get("players/nfl");

        var players = JsonSerializer.Deserialize<Dictionary<string, Player>>(await response.Content.ReadAsStreamAsync());
        await _playerCache.Cache(players);

        var player = players.GetValueOrDefault(playerId);
        return player;
    }

    public async Task<NflState> GetNflState()
    {
        var response = await Get("state/nfl");

        var nflState = JsonSerializer.Deserialize<NflState>(await response.Content.ReadAsStreamAsync());
        return nflState;
    }

    public async Task<List<Matchup>> GetMatchups(int week)
    {
        var response = await Get($"league/{_leagueId}/matchups/{week}");

        var matchups = JsonSerializer.Deserialize<List<Matchup>>(await response.Content.ReadAsStreamAsync());
        return matchups ?? [];
    }

    public async Task<Matchup?> GetMatchupByRosterId(int week, int rosterId)
    {
        var matchups = await GetMatchups(week);
        var userMatchups = matchups.FirstOrDefault(m => m.RosterId == rosterId);
        return userMatchups;
    }

    public async Task<Matchup> GetCoorespondingMatchup(int week, int matchupId)
    {
        var matchups = await GetMatchups(week);

        var thisMatchup = matchups.FirstOrDefault(m => m.MatchupId == matchupId);
        if (thisMatchup == null)
        {
            throw new Exception($"Matchup {matchupId} not found for week {week}");
        }

        var coorespondingMatchup = matchups
            .FirstOrDefault(m => m.MatchupId == matchupId &&
                                 m.RosterId != thisMatchup.RosterId);

        if (coorespondingMatchup == null)
        {
            throw new Exception($"Cooresponding matchup not found for matchup {matchupId} in week {week}");
        }

        return coorespondingMatchup;
    }
}
