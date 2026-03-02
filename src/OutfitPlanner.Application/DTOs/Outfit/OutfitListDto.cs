namespace OutfitPlanner.Application.DTOs.Outfit;

public class OutfitListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public int TimesWorn { get; set; }
    public DateTimeOffset? LastWorn { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty; // first item's thumbnail
}
