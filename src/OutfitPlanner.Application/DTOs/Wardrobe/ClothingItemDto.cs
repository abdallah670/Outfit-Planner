namespace OutfitPlanner.Application.DTOs.Wardrobe;

public class ClothingItemDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
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
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastWorn { get; set; }
    public int WearCount { get; set; }
    public DateTimeOffset? LastWashed { get; set; }
    public string MaintenanceNotes { get; set; } = string.Empty;
    public List<ClothingTagDto> Tags { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}

public class ClothingTagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = "manual";
    public decimal Confidence { get; set; }
}
