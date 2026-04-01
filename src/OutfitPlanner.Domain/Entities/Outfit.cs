using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;

public class Outfit : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public OccasionType Occasion { get; set; } 
    // Wait, diagram `string occasion` vs plan text `public OccasionType Occasion`. I'll use Enum.
    
    public string WeatherCondition { get; set; } = string.Empty;
    public Season Season { get; set; }
    public int? ComfortRating { get; set; }
    public int? StyleRating { get; set; }
    
    public DateTimeOffset? LastWorn { get; set; }
    public int TimesWorn { get; set; }
    public OutfitStatus Status { get; set; } = OutfitStatus.Active; // Enum
    // metadata jsonb? In SQL Server, likely nvarchar(max).
    // I'll skip metadata prop for now or use string.
    
    // Combined outfit image URL (stored permanently in database)
    public string? ImageUrl { get; set; }
    
    public ICollection<OutfitItem> Items { get; set; } = new List<OutfitItem>();
   // public ICollection<OutfitFeedback> Feedback { get; set; } = new List<OutfitFeedback>();
    public ICollection<PollOption> PollOptions { get; set; } = new List<PollOption>();
}

public class OutfitItem : BaseEntity
{
    public Guid OutfitId { get; set; }
    public Outfit Outfit { get; set; } = null!;
    
    public Guid ClothingItemId { get; set; }
    public ClothingItem ClothingItem { get; set; } = null!;
    
    public ItemRole Role { get; set; } // Primary, Secondary, Accent
    public int LayeringOrder { get; set; }
    public bool IsEssential { get; set; } = true;
}
