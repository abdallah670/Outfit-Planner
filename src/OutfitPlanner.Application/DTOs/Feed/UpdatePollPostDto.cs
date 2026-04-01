using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for updating a poll post
/// </summary>
public class UpdatePollPostDto
{
    public string? Question { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public Visibility Visibility { get; set; }
}
