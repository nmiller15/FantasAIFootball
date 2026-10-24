using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace StatsDashboard;

public class LeagueNameService(HttpClient client, IMemoryCache cache, ILogger<LeagueNameService> logger)
{
    public async Task<string> GetNameAsync(string leagueId)
    {
        // Sleeper league IDs are numeric. Don't make requests for missing or malformed IDs.
        if (!ulong.TryParse(leagueId, out _)) return leagueId;
        if (cache.TryGetValue(leagueId, out string? cached) && cached is not null) return cached;

        string? name = null;
        try
        {
            using var response = await client.GetAsync("league/" + Uri.EscapeDataString(leagueId));
            if (response.IsSuccessStatusCode)
            {
                var league = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (league.ValueKind == JsonValueKind.Object &&
                    league.TryGetProperty("name", out var value) && value.ValueKind == JsonValueKind.String)
                    name = value.GetString();
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Could not fetch Sleeper league name for {LeagueId}", leagueId);
        }

        // Cache failures briefly so a transient Sleeper outage doesn't delay every page view.
        var displayName = string.IsNullOrWhiteSpace(name) ? leagueId : name;
        cache.Set(leagueId, displayName, string.IsNullOrWhiteSpace(name)
            ? TimeSpan.FromMinutes(5) : TimeSpan.FromHours(6));
        return displayName;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetNamesAsync(IEnumerable<string> leagueIds)
    {
        var ids = leagueIds.Distinct().ToArray();
        var names = await Task.WhenAll(ids.Select(GetNameAsync));
        return ids.Zip(names).ToDictionary(pair => pair.First, pair => pair.Second);
    }
}
