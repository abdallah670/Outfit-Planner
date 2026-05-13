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

}
public class unCastVoteDto
{
    
    [Required]
    public Guid OptionId { get; set; }
}
