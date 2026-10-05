using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using FantasAIFootball.Logging;
using FantasAIFootball.Models.Search;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Repositories;

public class WebSearchRepository
{
    private readonly HttpClient _httpClient;

    public WebSearchRepository(IConfiguration configuration)
    {
        var tavilyKey = configuration.GetValue<string>("tavilyKey") ?? throw new Exception("Tavily API key is not set. Please set the 'tavilyKey' in your configuration.");

        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.tavily.com/"),
            DefaultRequestHeaders =
            {
                { "Authorization", $"Bearer {tavilyKey}" }
            }
        };
    }

    private async Task<HttpResponseMessage> Post(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body)
        };

        var url = $"{_httpClient.BaseAddress}{path}";

        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.SendAsync(request);
        stopwatch.Stop();

        if (response.IsSuccessStatusCode)
        {
            Log.Debug($"POST {url} → {(int)response.StatusCode} {response.StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
            return response;
        }

        Log.Error($"POST {url} → {(int)response.StatusCode} {response.StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<QueryResult?> Query(string query, DateTime? startDate = null, DateTime? endDate = null)
    {
        var response = await Post("search", new QueryRequestBody
        {
            Query = query,
            StartDate = startDate != null
                ? startDate.Value.ToString("yyyy-MM-dd")
                : DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"),
            EndDate = endDate != null
                ? endDate.Value.ToString("yyyy-MM-dd")
                : DateTime.Now.ToString("yyyy-MM-dd")
        });

        var result = JsonSerializer.Deserialize<QueryResult>(await response.Content.ReadAsStreamAsync());
        return result;
    }

    public async Task<List<ExtractResult>> Extract(List<string> urls)
    {
        var response = await Post("extract", new ExtractRequestBody
        {
            Urls = urls
        });

        var result = JsonSerializer.Deserialize<ExtractResponse>(await response.Content.ReadAsStreamAsync());
        return result?.Results ?? [];
    }
}
