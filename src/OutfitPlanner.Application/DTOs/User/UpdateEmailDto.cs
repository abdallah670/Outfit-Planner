using System.ComponentModel.DataAnnotations;

namespace OutfitPlanner.Application.DTOs.User;

public class UpdateEmailDto
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;
    
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}
