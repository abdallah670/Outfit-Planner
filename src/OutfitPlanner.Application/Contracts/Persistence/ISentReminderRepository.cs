using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface ISentReminderRepository : IGenericRepository<SentReminder>
{
    Task<bool> ExistsAsync(string userId, Guid calendarEventId, string reminderType, DateTimeOffset date);
}
