namespace OutfitPlanner.Application.DTOs.Outfit;

/// <summary>
/// DTO for creating a new outfit with a photo (no clothing items required)
/// </summary>
public class CreateOutfitWithPhotoDto
{
    public string Name { get; set; } = string.Empty;
    public string? Occasion { get; set; }
    public string? Season { get; set; }
    public string? WeatherCondition { get; set; }
    // Photo is sent as IFormFile in the controller, not in JSON body
}

/// <summary>
/// Response DTO for outfit created with photo
/// </summary>
public class CreateOutfitWithPhotoResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Occasion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
