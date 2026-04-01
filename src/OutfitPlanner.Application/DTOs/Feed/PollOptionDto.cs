using System;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for poll option information
/// </summary>
public class PollOptionDto
{
    public Guid Id { get; set; }
    public Guid? OutfitId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int VoteCount { get; set; }
    public string? OutfitThumbnail { get; set; }
}
