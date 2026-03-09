using System;
using System.ComponentModel.DataAnnotations;

namespace OutfitPlanner.Application.DTOs.Social;

/// <summary>
/// DTO for creating a poll option
/// </summary>
public class CreatePollOptionDto
{
    public Guid? OutfitId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
}
