using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.User;

public class UpdateUserProfileDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public UpdateStyleProfileDto? StyleProfile { get; set; }
    public UpdateUserPreferencesDto? Preferences { get; set; }
}

public class UpdateStyleProfileDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StylePreference? Style { get; set; }
    public List<string>? PreferredColors { get; set; }
    public string? FitPreferences { get; set; }
    public int? ComfortPriority { get; set; }
    public bool? AcceptsTrends { get; set; }
}

public class UpdateUserPreferencesDto
{
    public bool? ShareOutfitsAnonymously { get; set; }
    public bool? IncludeInTrendAnalysis { get; set; }
    public bool? AllowFriendRequests { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PrivacyLevel? DefaultOutfitPrivacy { get; set; }
    public bool? ShowBodyMetrics { get; set; }
    public bool? AllowLocationTracking { get; set; }
}
