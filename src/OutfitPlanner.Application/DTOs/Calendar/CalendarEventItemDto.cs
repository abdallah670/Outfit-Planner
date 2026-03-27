namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// DTO for calendar events (time-based events like "Team Meeting at 2:00 PM")
/// </summary>
public class CalendarEventItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset EventDate { get; set; }
    public string? StartTime { get; set; }
    public string? StartTimeDisplay { get; set; }
    public string? EndTime { get; set; }
    public string? EndTimeDisplay { get; set; }
    public CalendarEventType EventType { get; set; }
    public string EventTypeLabel => EventType.ToString();

    // Associated Outfit Info
    public Guid? WearEventId { get; set; }
    public string? OutfitName { get; set; }
    public string? OutfitImageUrl { get; set; }
    public bool HasOutfit => WearEventId.HasValue;

    // Metadata
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
}

/// <summary>
/// Request to create a new calendar event
/// </summary>
public class CreateCalendarEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset EventDate { get; set; }
    public string? StartTime { get; set; }  // Format: "HH:mm:ss" for API compatibility
    public string? EndTime { get; set; }    // Format: "HH:mm:ss" for API compatibility
    public CalendarEventType EventType { get; set; } = CalendarEventType.General;
    public Guid? OutfitId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing calendar event
/// </summary>
public class UpdateCalendarEventItemRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset? EventDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public CalendarEventType? EventType { get; set; }
    public Guid? OutfitId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Calendar event types for categorization
/// </summary>
public enum CalendarEventType
{
    General,
    Work,
    Meeting,
    Social,
    Date,
    Party,
    Sport,
    Travel,
    Appointment
}
