using System;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for vote information
/// </summary>
public class VoteDto
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
    public string VoterId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
