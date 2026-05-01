namespace OutfitPlanner.Application.DTOs.Wardrobe;

public class ClothingItemListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
    public int WearCount { get; set; }
    public DateTimeOffset? LastWorn { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;      // clothing type: "Top", "Bottom", etc.
    public decimal PurchasePrice { get; set; }
}
