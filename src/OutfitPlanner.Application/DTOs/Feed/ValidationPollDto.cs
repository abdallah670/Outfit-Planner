using System;
using System.Collections.Generic;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for returning validation poll information
/// </summary>
public class ValidationPollDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PollOptionDto> Options { get; set; } = new();
    public int TotalVotes { get; set; }
    public Guid? UserVotedOptionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
