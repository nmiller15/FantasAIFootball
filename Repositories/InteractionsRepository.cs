using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using FantasAIFootball.Models.Interactions;
using Microsoft.Extensions.Configuration;
using Serilog;

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

        Log.Debug($"post {_httpClient.BaseAddress}{request.RequestUri}");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Log.Information("post Interactions: {Path} - {StatusCode}", path, response.StatusCode);
            }
            else
            {
                Log.Error("post Interactions: {Path} - {StatusCode}\nERROR: {Response}",
                    path,
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync());
            }

            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
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
}
