using System;
using System.ComponentModel.DataAnnotations;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for casting a vote on a poll option
/// </summary>
public class CastVoteDto
{
    [Required]
    public Guid OptionId { get; set; }
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    public string? Comment { get; set; }
    
    public bool IsAnonymous { get; set; }
}
