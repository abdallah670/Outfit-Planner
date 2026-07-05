using System.Text;
using System.Text.Json;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class LLMResponseGenerator : ILLMResponseGenerator
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly ILogger<LLMResponseGenerator> _logger;

    public LLMResponseGenerator(HttpClient httpClient, IOptions<AISettings> settings, ILogger<LLMResponseGenerator> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<LLMResponse> GenerateResponseAsync(
        string userMessage,
        IntentResult intent,
        WardrobeContext context,
        OutfitCombinationResult combinations,
        ColorHarmonyResult harmony,
        StyleScoreResult styleScore,
        List<ChatHistoryEntry> recentHistory,
        List<string>? images = null,
        CancellationToken cancellationToken = default)
    {
        // Only short-circuit to a quick greeting if:
        // 1. The intent is a plain greeting or general query
        // 2. No occasion/weather context was detected
        // 3. No images were attached (if images present, always call the LLM so it can analyse them)
        var hasImages = images != null && images.Any();
        if (!hasImages &&
            intent.Intent is "greeting" or "general" &&
            string.IsNullOrEmpty(intent.Occasion) &&
            string.IsNullOrEmpty(intent.WeatherCondition))
        {
            return GenerateGreetingResponse();
        }

        // Build the prompt with context
        var prompt = BuildPrompt(userMessage, intent, context, combinations, harmony, styleScore, recentHistory);

        // Try the real LLM; fall back to GenerateFallbackResponse if anything fails
        try
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? 
                         Environment.GetEnvironmentVariable("AI__ApiKey") ??
                         _settings.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Gemini API key not found. Set GEMINI_API_KEY env var or configure AI:ApiKey in appsettings.json. Using fallback response.");
                return GenerateFallbackResponse(userMessage, intent, combinations, harmony, styleScore, context);
            }

            var provider = _settings.Provider?.ToLowerInvariant();

            if (provider == "gemini")
            {
                return await CallGeminiAsync(apiKey, prompt, images, cancellationToken);
            }
            else
            {
                return await CallOpenAiAsync(apiKey, prompt, images, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI API call failed. Using fallback response.");
            return GenerateFallbackResponse(userMessage, intent, combinations, harmony, styleScore, context);
        }
    }

    #region Gemini

    private async Task<LLMResponse> CallGeminiAsync(string apiKey, string prompt, List<string>? images, CancellationToken cancellationToken)
    {
        var model = string.IsNullOrEmpty(_settings.ModelName) ? "gemini-1.5-flash" : _settings.ModelName;
        var url = $"{_settings.Endpoint.TrimEnd('/')}/{model}:generateContent?key={apiKey}";

        // Build conversation history for Gemini (alternating user/model roles)
        var contents = new List<object>();
        // System instruction goes in its own field
        var systemPrompt = GetSystemPrompt();

        var parts = new List<object> { new { text = prompt } };
        
        if (images != null && images.Any())
        {
            foreach (var imgBase64 in images)
            {
                // Simple heuristic to determine mime type, defaulting to jpeg
                var mimeType = imgBase64.StartsWith("iVBORw0KGgo") ? "image/png" : "image/jpeg";
                parts.Add(new {
                    inline_data = new {
                        mime_type = mimeType,
                        data = imgBase64
                    }
                });
            }
        }

        // Add history messages (alternate user/model)
        // Split prompt into lines and parse conversation turns from the prompt
        contents.Add(new
        {
            role = "user",
            parts = parts.ToArray()
        });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = contents,
            generationConfig = new
            {
                maxOutputTokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _logger.LogInformation("Calling Gemini API: {Url}", url.Replace(apiKey, "***"));

        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeminiResponse>(responseBody);

        var text = result?.Candidates?.FirstOrDefault()
            ?.Content?.Parts?.FirstOrDefault()
            ?.Text
            ?? "I'm sorry, I couldn't process that request. Please try again.";

        _logger.LogInformation("Gemini response received: {Length} chars", text.Length);

        return new LLMResponse
        {
            Text = text,
            SuggestedActions = ExtractSuggestedActions("outfit_suggestion")
        };
    }

    #endregion

    #region OpenAI

    private async Task<LLMResponse> CallOpenAiAsync(string apiKey, string prompt, List<string>? images, CancellationToken cancellationToken)
    {
        object userContent = prompt;

        if (images != null && images.Any())
        {
            var contentParts = new List<object>
            {
                new { type = "text", text = prompt }
            };

            foreach (var img in images)
            {
                var mimeType = img.StartsWith("iVBORw0KGgo") ? "image/png" : "image/jpeg";
                contentParts.Add(new {
                    type = "image_url",
                    image_url = new { url = $"data:{mimeType};base64,{img}" }
                });
            }
            userContent = contentParts.ToArray();
        }

        var requestBody = new
        {
            model = _settings.ModelName,
            messages = new[]
            {
                new { role = "system", content = (object)GetSystemPrompt() },
                new { role = "user", content = userContent }
            },
            max_tokens = _settings.MaxTokens,
            temperature = _settings.Temperature
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

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
            SuggestedActions = ExtractSuggestedActions("outfit_suggestion")
        };
    }

    #endregion

    #region Prompts

    private string GetSystemPrompt() =>
        "You are a friendly and knowledgeable AI fashion assistant for Outfit Planner. " +
        "Help users with wardrobe, outfit selection, style advice, and fashion questions. " +
        "Be conversational, helpful, and concise. " +
        "When the user has wardrobe items mentioned below, reference them by name. " +
        "When the user sends an image of an outfit or clothing, ALWAYS analyse it in detail: " +
        "describe what you see, comment on the colour coordination, fit, style, and occasion suitability, " +
        "and give specific actionable suggestions to improve or complement the look. " +
        "Keep responses under 250 words. Use emojis occasionally.";

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
        sb.AppendLine($"User message: {userMessage}");
        sb.AppendLine($"Detected intent: {intent.Intent}");

        if (!string.IsNullOrEmpty(intent.Occasion))
            sb.AppendLine($"Occasion: {intent.Occasion}");
        if (!string.IsNullOrEmpty(intent.WeatherCondition))
            sb.AppendLine($"Weather: {intent.WeatherCondition}");

        if (context.AvailableItems.Any())
        {
            sb.AppendLine($"\nUser's wardrobe ({context.AvailableItems.Count} items total):");
            var grouped = context.AvailableItems
                .GroupBy(i => i.Category?.ToLower() ?? "other")
                .Select(g => $"{g.Count()} {g.Key}");
            sb.AppendLine(string.Join(", ", grouped));

            sb.AppendLine("\nTop items (max 10):");
            foreach (var item in context.AvailableItems.Take(10))
                sb.AppendLine($"- {item.Name} ({item.Category}) [{item.PrimaryColor}]");
        }
        else
        {
            sb.AppendLine("\nUser has no wardrobe items yet.");
        }

        if (combinations.Combinations.Any())
        {
            sb.AppendLine("\nSuggested outfit combinations (use these as the primary recommendation):");
            foreach (var c in combinations.Combinations)
                sb.AppendLine($"#{c.Rank}: {string.Join(", ", c.Items.Select(i => i.Name))} Score: {c.TotalScore:F1}/100");
        }

        if (!string.IsNullOrEmpty(harmony.Explanation))
            sb.AppendLine($"\nColor harmony: {harmony.Scheme} ({harmony.Score:F1}/100) - {harmony.Explanation}");
        if (styleScore.TotalScore > 0)
            sb.AppendLine($"Style compatibility score: {styleScore.TotalScore:F1}/100");

        sb.AppendLine("\nRespond in a friendly, helpful tone. Suggest specific items from the user's wardrobe when possible.");

        return sb.ToString();
    }

    #endregion

    #region Greeting & Fallback

    private static LLMResponse GenerateGreetingResponse()
    {
        var texts = new[]
        {
            "Hi there! 👋 I'm your AI fashion assistant. Ask me for outfit ideas, a wardrobe review, or trip packing help!",
            "Hey! 👋 What can I help you with today? Try asking for an outfit suggestion, a style tip, or help planning what to wear!",
            "Hello! 🌟 I'm here to help you look your best. Want me to suggest an outfit from your wardrobe, or just have a style question?"
        };
        return new LLMResponse
        {
            Text = texts[Random.Shared.Next(texts.Length)],
            SuggestedActions = new List<string> { "Ask more", "View wardrobe", "Help" }
        };
    }

    private static LLMResponse GenerateFallbackResponse(
        string userMessage,
        IntentResult intent,
        OutfitCombinationResult combinations,
        ColorHarmonyResult harmony,
        StyleScoreResult styleScore,
        WardrobeContext context)
    {
        var text = intent.Intent switch
        {
            "outfit_suggestion" => combinations.Combinations.Any()
                ? $"Here's an outfit suggestion: {string.Join(", ", combinations.Combinations.First().Items.Select(i => i.Name))}. " +
                  $"This combination scores {combinations.Combinations.First().TotalScore:F1}/100!"
                : context.AvailableItems.Any()
                    ? $"I see you have {context.AvailableItems.Count} items in your wardrobe. Try pairing a {context.AvailableItems.First().Name} with jeans and comfortable shoes for a great look!"
                    : "Try a classic combination: a nice top with well-fitted jeans and comfortable shoes! 👖👟",
            "outfit_rating" => styleScore.TotalScore > 0
                ? $"Your outfit scores {styleScore.TotalScore:F1}/100. {harmony.Explanation}"
                : "Your outfit looks great! Try adding a statement accessory to complete the look.",
            "wardrobe_analysis" => context.AvailableItems.Any()
                ? $"You have {context.AvailableItems.Count} items in your wardrobe: " +
                  $"{string.Join(", ", context.AvailableItems.GroupBy(i => i.Category).Select(g => $"{g.Count()} {g.Key}"))}. " +
                  "Consider adding accessories to complete your looks!"
                : "You don't have any items in your wardrobe yet. Start by adding some clothing items!",
            "trip_planning" => "Pack versatile pieces you can mix and match for your trip! Include comfortable shoes and layers for changing weather.",
            "style_query" => "That's a great style question! Consider choosing pieces that reflect your personal taste while keeping the occasion in mind.",
            "weather_query" => "Great question! Always check the weather first - layering is key for changing conditions.",
            _ => $"Hi! I'm your fashion assistant. You can ask me for outfit ideas, a wardrobe review, or style advice. How can I help? 👗👔"
        };

        return new LLMResponse
        {
            Text = text,
            SuggestedActions = ExtractSuggestedActions(intent.Intent)
        };
    }

    #endregion

    private static List<string> ExtractSuggestedActions(string intent) => intent switch
    {
        "outfit_suggestion" => new List<string> { "Save outfit", "View wardrobe", "Check weather" },
        "outfit_rating" => new List<string> { "Try different", "Check colors" },
        "wardrobe_analysis" => new List<string> { "View wardrobe", "Add items", "Statistics" },
        "trip_planning" => new List<string> { "Pack outfits", "Check weather", "Calendar" },
        "style_query" => new List<string> { "Explore colors", "Style tips" },
        _ => new List<string> { "Ask more", "View wardrobe", "Help" }
    };

    #region DTOs

    private class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private class GeminiPart
    {
        public string? Text { get; set; }
    }

    private class OpenAiResponse { public List<Choice>? Choices { get; set; } }
    private class Choice { public Message? Message { get; set; } }
    private class Message { public string? Content { get; set; } }

    #endregion
}