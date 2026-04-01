namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// DTO for calendar events (wear events)
/// </summary>
public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid? OutfitId { get; set; }
    public string? OutfitName { get; set; }
    public string? OutfitImageUrl { get; set; }
    public Guid? ClothingItemId { get; set; }
    public DateTimeOffset WornAt { get; set; }
    public int DurationMinutes { get; set; }
    public string WeatherCondition { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsScheduled { get; set; }
}

public class TodayEventDto
{
    public string Title { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; }
}
