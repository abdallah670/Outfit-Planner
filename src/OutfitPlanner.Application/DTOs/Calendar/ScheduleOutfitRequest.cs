namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// Request to schedule an outfit
/// </summary>
public class ScheduleOutfitRequest
{
    public Guid OutfitId { get; set; }
    public DateTimeOffset ScheduledDate { get; set; }
    public DateTimeOffset ScheduledAt => ScheduledDate;
    public Guid? ClothingItemId { get; set; }
    public string? WeatherCondition { get; set; }
    public string? Occasion { get; set; }
    public string? Notes { get; set; }
}
