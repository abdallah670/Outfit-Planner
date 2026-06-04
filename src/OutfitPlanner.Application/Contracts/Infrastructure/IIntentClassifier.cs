namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class IntentResult
{
    public string Intent { get; set; } = string.Empty; // outfit_suggestion, outfit_rating, wardrobe_analysis, trip_planning, style_query, general
    public string? Occasion { get; set; }
    public string? WeatherCondition { get; set; }
    public string? Season { get; set; }
    public List<string>? MentionedItems { get; set; }
}

public interface IIntentClassifier
{
    Task<IntentResult> ClassifyAsync(string message, CancellationToken cancellationToken = default);
}