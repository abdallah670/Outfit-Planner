using System.Text;
using System.Text.Json;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class LLMResponseGenerator : ILLMResponseGenerator
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly IConfiguration _configuration;

    public LLMResponseGenerator(HttpClient httpClient, IOptions<AISettings> settings, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _configuration = configuration;
    }

    public async Task<LLMResponse> GenerateResponseAsync(
        string userMessage,
        IntentResult intent,
        WardrobeContext context,
        OutfitCombinationResult combinations,
        ColorHarmonyResult harmony,
        StyleScoreResult styleScore,
        List<ChatHistoryEntry> recentHistory,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(userMessage, intent, context, combinations, harmony, styleScore, recentHistory);

        try
        {
            // Get API key from environment variable first, fallback to settings
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
                        Environment.GetEnvironmentVariable("AI__ApiKey") ??
                        _settings.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("AI API key not found. Set OPENAI_API_KEY environment variable or configure in appsettings.json");
            }

            var requestBody = new
            {
                model = _settings.ModelName,
                messages = new[]
                {
                    new { role = "system", content = GetSystemPrompt() },
                    new { role = "user", content = prompt }
                },
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            // Add API key to headers
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(_settings.Endpoint, jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseBody);

            var text = result?.Choices?.FirstOrDefault()?.Message?.Content
                ?? "I'm sorry, I couldn't process that request. Please try again.";

            return new LLMResponse
            {
                Text = text,
                SuggestedActions = ExtractSuggestedActions(intent.Intent)
            };
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"AI API Error: {ex.Message}");
            return GenerateFallbackResponse(userMessage, intent, combinations, harmony, styleScore);
        }
    }

    private static string GetSystemPrompt() =>
        "You are a friendly and knowledgeable AI fashion assistant for Outfit Planner. " +
        "Help users with wardrobe, outfit selection, style advice, and fashion questions. " +
        "Be conversational, helpful, and concise. Reference the user's actual wardrobe items. " +
        "Keep responses under 200 words. Use emojis occasionally.";

    private static string BuildPrompt(
        string userMessage,
        IntentResult intent,
        WardrobeContext context,
        OutfitCombinationResult combinations,
        ColorHarmonyResult harmony,
        StyleScoreResult styleScore,
        List<ChatHistoryEntry> history)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"User: {userMessage}");
        sb.AppendLine($"Intent: {intent.Intent}");

        if (context.AvailableItems.Any())
        {
            sb.AppendLine("\nWardrobe items:");
            foreach (var item in context.AvailableItems.Take(5))
                sb.AppendLine($"- {item.Name} ({item.Category}) [{item.PrimaryColor}]");
        }

        if (combinations.Combinations.Any())
        {
            sb.AppendLine("\nSuggested outfits:");
            foreach (var c in combinations.Combinations)
                sb.AppendLine($"#{c.Rank}: {string.Join(", ", c.Items.Select(i => i.Name))} Score: {c.TotalScore}");
        }

        if (!string.IsNullOrEmpty(harmony.Explanation))
            sb.AppendLine($"\nColors: {harmony.Scheme} ({harmony.Score}/100)");

        if (history.Any())
        {
            sb.AppendLine("\nHistory:");
            foreach (var h in history.TakeLast(3))
                sb.AppendLine($"- {h.Role}: {h.Content[..Math.Min(h.Content.Length, 100)]}");
        }

        return sb.ToString();
    }

    private static List<string> ExtractSuggestedActions(string intent) => intent switch
    {
        "outfit_suggestion" => new List<string> { "Save outfit", "View wardrobe", "Check weather" },
        "outfit_rating" => new List<string> { "Try different", "Check colors" },
        "wardrobe_analysis" => new List<string> { "View wardrobe", "Add items", "Statistics" },
        "trip_planning" => new List<string> { "Pack outfits", "Check weather", "Calendar" },
        "style_query" => new List<string> { "Explore colors", "Style tips" },
        _ => new List<string> { "Ask more", "View wardrobe", "Help" }
    };

    private static LLMResponse GenerateFallbackResponse(
        string userMessage,
        IntentResult intent,
        OutfitCombinationResult combinations,
        ColorHarmonyResult harmony,
        StyleScoreResult styleScore)
    {
        var text = intent.Intent switch
        {
            "outfit_suggestion" => combinations.Combinations.Any()
                ? $"Here's an outfit: {string.Join(", ", combinations.Combinations.First().Items.Select(i => i.Name))}. Score: {combinations.Combinations.First().TotalScore}/100!"
                : "Try a top with jeans and comfortable shoes!",
            "outfit_rating" => $"Your outfit scores {styleScore.TotalScore}/100. {harmony.Explanation}",
            "wardrobe_analysis" => "You have a nice variety! Consider adding accessories.",
            "trip_planning" => "Pack versatile pieces you can mix and match for your trip!",
            _ => $"I understand you're asking about '{userMessage}'. Could you tell me more?"
        };

        return new LLMResponse
        {
            Text = text,
            SuggestedActions = ExtractSuggestedActions(intent.Intent)
        };
    }

    private class OpenAiResponse { public List<Choice>? Choices { get; set; } }
    private class Choice { public Message? Message { get; set; } }
    private class Message { public string? Content { get; set; } }
}