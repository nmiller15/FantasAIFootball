using System.Net.Http.Json;
using System.Text.Json;
using FantasAIFootball.Models.Interactions;
using Microsoft.Extensions.Configuration;

namespace FantasAIFootball.Repositories;

public class InteractionsRepository
{
    private readonly HttpClient _httpClient;

    public readonly string _model;
    private readonly bool _debug;

    public InteractionsRepository(IConfiguration configuration)
    {
        var apiKey = configuration.GetValue<string>("geminiKey") ?? throw new Exception("Gemini API key is not set. Please set the 'geminiKey' in your configuration.");

        _model = configuration.GetValue<string>("model") ?? throw new Exception("Model is not set. Please set the 'model' in your configuration.");
        _debug = configuration.GetValue("debug", false);

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
        if (_debug)
        {
            Console.WriteLine(json);
        }

        var result = JsonSerializer.Deserialize<Interaction>(json);
        return result;
    }

    private async Task<HttpResponseMessage> Post(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body)
        };

        if (_debug)
        {
            Console.Write($"POST {_httpClient.BaseAddress}{request.RequestUri}");
        }

        var response = await _httpClient.SendAsync(request);

        if (_debug)
        {
            Console.Write(" - ");
            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(response.StatusCode);
            Console.ResetColor();

            if (!response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: ");
                Console.ResetColor();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

        response.EnsureSuccessStatusCode();
        return response;
    }
}
