using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Domain.Entities;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLogin { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    
    // Navigation properties
    public UserStyleProfile? StyleProfile { get; set; }
    public UserPreferences? Preferences { get; set; }
    public ICollection<ClothingItem> ClothingItems { get; set; } = new List<ClothingItem>();
    public ICollection<Outfit> Outfits { get; set; } = new List<Outfit>();
    public ICollection<ValidationPoll> Polls { get; set; } = new List<ValidationPoll>();
    public ICollection<WearEvent> WearEvents { get; set; } = new List<WearEvent>();
    public ICollection<FeedPost> FeedPosts { get; set; } = new List<FeedPost>();
    public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
}

public class UserStyleProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    // public User User { get; set; } = null!; // Circular reference handling might be needed in configurations
    
    public StylePreference Style { get; set; }
    public List<string> PreferredColors { get; set; } = new();
    public string FitPreferences { get; set; } = string.Empty;
    public int ComfortPriority { get; set; } // 0-100
    public bool AcceptsTrends { get; set; }
    public ICollection<StyleRule> CustomRules { get; set; } = new List<StyleRule>();
}

public class UserPreferences : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    // public User User { get; set; } = null!;

    public bool ShareOutfitsAnonymously { get; set; }
    public bool IncludeInTrendAnalysis { get; set; } = true;
    public bool AllowFriendRequests { get; set; } = true;
    public PrivacyLevel DefaultOutfitPrivacy { get; set; } = PrivacyLevel.Private;
    public bool ShowBodyMetrics { get; set; }
    public bool AllowLocationTracking { get; set; } = true;
    // DataRetentionPeriod not defined - skipped or add later
}
