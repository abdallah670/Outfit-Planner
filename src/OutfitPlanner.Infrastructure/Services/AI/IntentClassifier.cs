using System.Text.Json;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;
using Microsoft.Extensions.Options;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class IntentClassifier : IIntentClassifier
{
    private static readonly Dictionary<string, string[]> IntentKeywords = new()
    {
        ["greeting"] = new[] { "hi", "hello", "hey", "good morning", "good evening", "good afternoon", "howdy", "sup" },
        ["outfit_suggestion"] = new[] { "wear", "outfit", "put on", "dress", "what should", "what to", "suggest", "recommend" },
        ["outfit_rating"] = new[] { "rate", "rating", "score", "evaluate", "how does", "look good", "review" },
        ["wardrobe_analysis"] = new[] { "missing", "need", "wardrobe", "closet", "analysis", "analyze", "inventory", "have", "statistics", "stats" },
        ["style_query"] = new[] { "style", "trend", "fashion", "color", "match", "go with", "pair", "my style", "style profile" },
        ["weather_query"] = new[] { "weather", "rain", "cold", "hot", "warm", "temperature", "forecast" },
        ["calendar_action"] = new[] { "wear this today", "schedule", "add to calendar", "wear event", "log wear" },
        ["save_action"] = new[] { "save outfit", "save this", "bookmark", "keep this" },
    };

    private static readonly Dictionary<string, string[]> OccasionKeywords = new()
    {
        ["casual"] = new[] { "casual", "everyday", "relaxed", "weekend", "hanging", "chill" },
        ["formal"] = new[] { "formal", "gala", "black tie", "elegant", "sophisticated" },
        ["business"] = new[] { "business", "office", "work", "corporate", "professional", "meeting", "interview" },
        ["beach"] = new[] { "beach", "swim", "pool", "vacation", "tropical" },
        ["party"] = new[] { "party", "club", "night", "dancing", "celebration" },
        ["date"] = new[] { "date", "romantic", "dinner", "valentine" },
        ["workout"] = new[] { "workout", "gym", "exercise", "sport", "running", "athletic" },
        ["outdoor"] = new[] { "outdoor", "hike", "camp", "park", "nature", "walk" }
    };

    private static readonly Dictionary<string, string[]> WeatherKeywords = new()
    {
        ["rainy"] = new[] { "rain", "rainy", "wet", "storm", "umbrella" },
        ["cold"] = new[] { "cold", "freezing", "snow", "winter", "chilly", "frost" },
        ["hot"] = new[] { "hot", "heat", "summer", "warm", "sunny" },
        ["windy"] = new[] { "windy", "wind" },
    };

    private static readonly string[] ClothingKeywords = new[]
    {
        "shirt", "pants", "shoes", "jacket", "dress", "skirt", "coat", "hat", "scarf",
        "boots", "sneakers", "blazer", "suit", "tie", "belt", "socks", "jeans", "shorts",
        "sweater", "hoodie", "t-shirt", "polo", "blouse", "heels", "sandals", "gloves"
    };

    public Task<IntentResult> ClassifyAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var lower = message.ToLowerInvariant();
        var words = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var intent = ClassifyIntent(lower, words);
        var occasion = DetectOccasion(lower, words);
        var weather = DetectWeather(lower, words);
        var mentionedItems = DetectClothingItems(words);

        return Task.FromResult(new IntentResult
        {
            Intent = intent,
            Occasion = occasion,
            WeatherCondition = weather,
            Season = null,
            MentionedItems = mentionedItems
        });
    }

    private static string ClassifyIntent(string text, string[] words)
    {
        var scores = new Dictionary<string, int>();
        foreach (var (intent, keywords) in IntentKeywords)
        {
            scores[intent] = keywords.Sum(kw => text.Contains(kw) ? kw.Split(' ').Length : 0);
        }
        return scores.MaxBy(kv => kv.Value).Value > 0
            ? scores.MaxBy(kv => kv.Value).Key
            : "general";
    }

    private static string? DetectOccasion(string text, string[] words)
    {
        foreach (var (occasion, keywords) in OccasionKeywords)
        {
            if (keywords.Any(kw => text.Contains(kw)))
                return occasion;
        }
        return null;
    }

    private static string? DetectWeather(string text, string[] words)
    {
        foreach (var (weather, keywords) in WeatherKeywords)
        {
            if (keywords.Any(kw => text.Contains(kw)))
                return weather;
        }
        return null;
    }

    private static List<string> DetectClothingItems(string[] words)
    {
        return words.Where(w => ClothingKeywords.Contains(w)).Distinct().ToList();
    }
}