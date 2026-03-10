namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// DTO for scheduled outfits
/// </summary>
public class ScheduledOutfitDto
{
    public Guid Id { get; set; }
    public Guid OutfitId { get; set; }
    public string OutfitName { get; set; } = string.Empty;
    public string? OutfitImageUrl { get; set; }
    public string? Occasion { get; set; }
    public DateTimeOffset ScheduledDate { get; set; }
    public string? Notes { get; set; }
    public bool Worn { get; set; }
}
