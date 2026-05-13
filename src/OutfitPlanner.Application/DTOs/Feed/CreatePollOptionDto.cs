using System;
using System.ComponentModel.DataAnnotations;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for creating a poll option
/// </summary>
public class CreatePollOptionDto
{
    public Guid? OutfitId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 1)]
    
    
    public int DisplayOrder { get; set; }
}
