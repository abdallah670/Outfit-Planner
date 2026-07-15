using System;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// Represents a user mentioned in a comment (userId + display name + avatar).
/// Stored as a JSON list on the PostComment entity.
/// </summary>
public class MentionedUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;   // full/display name (User.Name)
    public string? ProfilePictureUrl { get; set; }
}
