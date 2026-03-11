using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository for calendar events
/// </summary>
public interface ICalendarEventRepository : IGenericRepository<CalendarEvent>
{
    Task<IEnumerable<CalendarEvent>> GetByUserIdAndDateAsync(string userId, DateTimeOffset date);
    Task<IEnumerable<CalendarEvent>> GetByUserIdAndMonthAsync(string userId, int year, int month);
}
