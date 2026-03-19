using System.Text.Json.Serialization;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.User;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    
    // Stats
    public int WardrobeItemCount { get; set; }
    public int OutfitCount { get; set; }
    public int TotalWears { get; set; }
    
    // Style Profile
    public UserStyleProfileDto? StyleProfile { get; set; }
    
    // Preferences
    public UserPreferencesDto? Preferences { get; set; }
}

public class UserStyleProfileDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StylePreference Style { get; set; }
    public List<string> PreferredColors { get; set; } = new();
    public string FitPreferences { get; set; } = string.Empty;
    public int ComfortPriority { get; set; }
    public bool AcceptsTrends { get; set; }
    public List<StyleRuleDto> CustomRules { get; set; } = new();
}

public class UserPreferencesDto
{
    public bool ShareOutfitsAnonymously { get; set; }
    public bool IncludeInTrendAnalysis { get; set; }
    public bool AllowFriendRequests { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PrivacyLevel DefaultOutfitPrivacy { get; set; }
    public bool ShowBodyMetrics { get; set; }
    public bool AllowLocationTracking { get; set; }
}
