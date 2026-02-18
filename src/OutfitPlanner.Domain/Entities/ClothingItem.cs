using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.ValueObjects;

namespace OutfitPlanner.Domain.Entities;

public class ClothingItem : BaseEntity
{
    public string UserId { get; set; } = string.Empty; // Changed to string for IdentityUser key
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public ClothingType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public List<string> SecondaryColors { get; set; } = new();
    public FabricType Fabric { get; set; }
    public string Brand { get; set; } = string.Empty;
    public Money PurchasePrice { get; set; } = Money.From(0, "USD");
    public DateTime? PurchaseDate { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Condition { get; set; } = "good"; // Enum or string? Diagram says string with default 'good'
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastWorn { get; set; }
    public int WearCount { get; set; }
    public DateTimeOffset? LastWashed { get; set; }
    public string MaintenanceNotes { get; set; } = string.Empty;

    public ICollection<ClothingTag> Tags { get; set; } = new List<ClothingTag>();
    public ICollection<OutfitItem> OutfitItems { get; set; } = new List<OutfitItem>();
    public ICollection<WearEvent> WearEvents { get; set; } = new List<WearEvent>();
}

public class ClothingTag : BaseEntity
{
    public Guid ClothingItemId { get; set; }
    public ClothingItem ClothingItem { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty; // TagName in diagram
    public string Source { get; set; } = "manual"; // ai, manual, community
    public decimal Confidence { get; set; }
}
