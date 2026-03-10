namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// Request to update a calendar event
/// </summary>
public class UpdateCalendarEventRequest
{
    public DateTimeOffset? WornAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? WeatherCondition { get; set; }
    public int? Rating { get; set; }
    public string? Notes { get; set; }
}
