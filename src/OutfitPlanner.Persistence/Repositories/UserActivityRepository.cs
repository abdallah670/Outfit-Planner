using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class UserActivityRepository : GenericRepository<UserActivity>, IUserActivityRepository
{
    public UserActivityRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserActivity>> GetByUserIdAsync(string userId, int limit = 100)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserActivity>> GetByTypeAsync(ActivityType type, int limit = 100)
    {
        return await _dbSet
            .Where(a => a.Type == type)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserActivity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<int> GetCountByTypeAsync(ActivityType type)
    {
        return await _dbSet
            .CountAsync(a => a.Type == type);
    }

    public async Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 50)
    {
        return await _dbSet
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}
