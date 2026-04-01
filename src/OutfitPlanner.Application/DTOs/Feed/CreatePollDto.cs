using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutfitPlanner.Application.DTOs.Social;

/// <summary>
/// DTO for creating a new validation poll
/// </summary>
public class CreatePollDto
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Question { get; set; } = string.Empty;
    
    public string? Context { get; set; }
    
    [Required]
    public DateTimeOffset ExpiresAt { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 options are required")]
    public List<CreatePollOptionDto> Options { get; set; } = new();
}
