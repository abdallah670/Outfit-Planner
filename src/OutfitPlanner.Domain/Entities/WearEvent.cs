namespace OutfitPlanner.Domain.Entities;

public class WearEvent : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public Guid? ClothingItemId { get; set; }
    public ClothingItem? ClothingItem { get; set; }
    
    public Guid? OutfitId { get; set; }
    public Outfit? Outfit { get; set; }
    
    public Guid? EventId { get; set; }
    
    public DateTimeOffset WornAt { get; set; }
    public int DurationMinutes { get; set; }
    public string WeatherCondition { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Notes { get; set; } = string.Empty;
}
