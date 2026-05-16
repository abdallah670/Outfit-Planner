using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

/// <summary>
/// DTO for updating an outfit post
/// </summary>
public class UpdateOutfitPostDto
{
    public string? Caption { get; set; }
    public Visibility Visibility { get; set; }
    public List<string> Tags { get; set; }
    public Guid OutfitId { get; set; }
}
