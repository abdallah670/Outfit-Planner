using Microsoft.AspNetCore.Http;

namespace OutfitPlanner.Application.DTOs.Outfit;

/// <summary>
/// DTO for creating a new outfit with a photo (no clothing items required)
/// </summary>
public class CreateOutfitWithPhotoDto
{
    public string? Name { get; set; }
    public IFormFile? Photo { get; set; }
}

/// <summary>
/// Response DTO for outfit created with photo
/// </summary>
public class CreateOutfitWithPhotoResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    
    public string? ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
