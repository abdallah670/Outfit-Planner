using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class SentReminderRepository : GenericRepository<SentReminder>, ISentReminderRepository
{
    public SentReminderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(string userId, Guid calendarEventId, string reminderType, DateTimeOffset date)
    {
        var targetDate = date.Date;
        return await _context.SentReminders.AnyAsync(sr =>
            sr.UserId == userId &&
            sr.CalendarEventId == calendarEventId &&
            sr.ReminderType == reminderType &&
            sr.SentAt.Date == targetDate);
    }
}
