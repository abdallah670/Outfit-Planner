namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class OutfitCombinationResult
{
    public List<OutfitCombinationItem> Combinations { get; set; } = new();
    public string? Error { get; set; }
}

public class OutfitCombinationItem
{
    public int Rank { get; set; }
    public List<ClothingItemRef> Items { get; set; } = new();
    public double TotalScore { get; set; }
    public Dictionary<string, double> ScoreBreakdown { get; set; } = new();
}

public class ClothingItemRef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
}

public interface IOutfitCombinationService
{
    Task<OutfitCombinationResult> GenerateCombinationsAsync(
        string userId,
        string? occasion,
        string? weatherCondition,
        int maxResults = 3,
        CancellationToken cancellationToken = default);
}