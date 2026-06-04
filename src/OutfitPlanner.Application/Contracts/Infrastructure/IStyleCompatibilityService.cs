namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class StyleScoreResult
{
    public double TotalScore { get; set; }
    public Dictionary<string, double> Breakdown { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}

public interface IStyleCompatibilityService
{
    Task<StyleScoreResult> CalculateScoreAsync(
        IEnumerable<string> clothingItemIds,
        string occasion,
        string? weatherCondition,
        string userId,
        CancellationToken cancellationToken = default);
}