namespace OutfitPlanner.Application.DTOs.Wardrobe;

public class UpdateClothingItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public List<string> SecondaryColors { get; set; } = new();
    public string Fabric { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? PurchaseDate { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Condition { get; set; } = "good";
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string MaintenanceNotes { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
