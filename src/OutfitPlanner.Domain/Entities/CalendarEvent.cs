namespace OutfitPlanner.Domain.Entities;

/// <summary>
/// Represents a calendar event for outfit planning with time slots
/// e.g., "Team Meeting at 2:00 PM"
/// </summary>
public class CalendarEvent : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    // Event Details
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }

    // Time Scheduling
    public DateTimeOffset EventDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // Event Type
    public CalendarEventType EventType { get; set; } = CalendarEventType.General;

    // Associated Outfit (optional - can have event without outfit)
    public Guid? WearEventId { get; set; }
    public WearEvent? WearEvent { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; } // JSON serialized RecurrencePattern
}



/// <summary>
/// Value object for recurrence pattern
/// </summary>
public class RecurrencePattern
{
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; } = 1;
    public List<DayOfWeek>? DaysOfWeek { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

