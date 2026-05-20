using System.Text.Json.Serialization;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.User;

/// <summary>
/// Publicly viewable user profile data (non-sensitive)
/// </summary>
public class PublicUserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("username")]
    public string UserName { get; set; } = string.Empty; // Handle (@username)
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsFollowing { get; set; } = false; // Added for frontend
    public bool IsOwner { get; set; } = false; // Added for frontend

    // Stats
    public int OutfitCount { get; set; }
    public int WardrobeItemCount { get; set; }
    public int TotalWears { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }

    // Style Profile (optional summary)
    public PublicUserStyleProfileDto? StyleProfile { get; set; }
}

public class PublicUserStyleProfileDto
{
    public StylePreference Style { get; set; }
    public List<string> PreferredColors { get; set; } = new();
    public string? FitPreferences { get; set; }
    public int ComfortPriority { get; set; }
    public bool AcceptsTrends { get; set; }
}
