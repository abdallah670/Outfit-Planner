namespace OutfitPlanner.Domain.Entities;

public class SentReminder : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid? CalendarEventId { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
}
