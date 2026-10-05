using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using FantasAIFootball.Logging;
using FantasAIFootball.Models.Interactions;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Repositories;

public class InteractionsRepository
{
    private readonly HttpClient _httpClient;

    public readonly string _model;

    public InteractionsRepository(IConfiguration configuration)
    {
        var apiKey = configuration.GetValue<string>("geminiKey") ?? throw new Exception("Gemini API key is not set. Please set the 'geminiKey' in your configuration.");

        _model = configuration.GetValue<string>("model") ?? throw new Exception("Model is not set. Please set the 'model' in your configuration.");

        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1/"),
            DefaultRequestHeaders =
            {
                { "x-goog-api-key", apiKey }
            }
        };
    }

    public async Task<Interaction> Create(InteractionRequestBody requestBody)
    {
        var response = await Post("interactions", requestBody);

        var json = await response.Content.ReadAsStringAsync();
        Log.Debug(json);

        var result = JsonSerializer.Deserialize<Interaction>(json);
        return result;
    }

    private async Task<HttpResponseMessage> Post(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body)
        };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            var url = $"{_httpClient.BaseAddress}{path}";

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var delay = response.Headers?.RetryAfter?.Delta
                    ?? TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt));

                response.Dispose();

                Log.Warn($"Rate limited on {url}. Retrying in {delay.TotalSeconds:0.#} seconds...");
                await Task.Delay(delay);
                continue;
            }

            if (response.IsSuccessStatusCode)
            {
                Log.Debug($"POST {url} → {(int)response.StatusCode} {response.StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
                return response;
            }

            Log.Error($"POST {url} → {(int)response.StatusCode} {response.StatusCode} ({stopwatch.ElapsedMilliseconds} ms){Environment.NewLine}ERROR: {await response.Content.ReadAsStringAsync()}");
            response.EnsureSuccessStatusCode();
            return response;
        }

        throw new UnreachableException();
    }
}
