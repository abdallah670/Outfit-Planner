using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

/// <summary>
/// Repository for calendar events
/// </summary>
public class CalendarEventRepository : GenericRepository<CalendarEvent>, ICalendarEventRepository
{
    public CalendarEventRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CalendarEvent>> GetByUserIdAndDateAsync(string userId, DateTimeOffset date)
    {
        return await _dbSet
            .Include(e => e.WearEvent)
                .ThenInclude(we => we!.Outfit)
            .Where(e => e.UserId == userId && e.EventDate.Date == date.Date)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<CalendarEvent>> GetByUserIdAndMonthAsync(string userId, int year, int month)
    {
        return await _dbSet
            .Include(e => e.WearEvent)
                .ThenInclude(we => we!.Outfit)
            .Where(e => e.UserId == userId && e.EventDate.Year == year && e.EventDate.Month == month)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<CalendarEvent?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.WearEvent)
                .ThenInclude(we => we!.Outfit)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
