namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class WardrobeContext
{
    public string UserId { get; set; } = string.Empty;
    public string UserStyleProfile { get; set; } = string.Empty;
    public string? WeatherForecast { get; set; }
    public List<WardrobeItemContext> AvailableItems { get; set; } = new();
    public List<string> RecentlyWornItemIds { get; set; } = new();
}

public class WardrobeItemContext
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string? SecondaryColors { get; set; }
    public string? Fabric { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public int WearCount { get; set; }
}

public interface IWardrobeContextBuilder
{
    Task<WardrobeContext> BuildContextAsync(
        string userId,
        IntentResult intent,
        CancellationToken cancellationToken = default);
}