using System.ComponentModel.DataAnnotations;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.User;

public class UpdateUserProfileDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public UserStyleProfileDto? StyleProfile { get; set; }
    public UserPreferencesDto? Preferences { get; set; }
}

public class UpdateStyleProfileDto
{
    public StylePreference Style { get; set; }
    public List<string> PreferredColors { get; set; } = new();
    public string FitPreferences { get; set; } = string.Empty;
    public int ComfortPriority { get; set; }
    public bool AcceptsTrends { get; set; }
}

public class UpdateUserPreferencesDto
{
    public bool ShareOutfitsAnonymously { get; set; }
    public bool IncludeInTrendAnalysis { get; set; }
    public bool AllowFriendRequests { get; set; }
    public PrivacyLevel DefaultOutfitPrivacy { get; set; }
    public bool ShowBodyMetrics { get; set; }
    public bool AllowLocationTracking { get; set; }
}
