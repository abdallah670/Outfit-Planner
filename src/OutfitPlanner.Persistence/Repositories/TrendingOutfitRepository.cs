using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Persistence.Repositories;

public class TrendingOutfitRepository : GenericRepository<TrendingOutfit>, ITrendingOutfitRepository
{
    public TrendingOutfitRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TrendingOutfit>> GetTrendingByLocationAsync(string location, int count = 10)
    {
        return await _dbSet
            .Include(t => t.Outfit)
                .ThenInclude(o => o.User)
            .Include(t => t.Poll)
            .Where(t => t.Date.Date == DateTime.Today)
            .OrderByDescending(t => t.TrendingScore)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrendingOutfit>> GetGlobalTrendingAsync(int count = 20)
    {
        // Only return recent trending data (last 7 days) and order by score
        var cutoffDate = DateTime.Today.AddDays(-7);
        return await _dbSet
            .Include(t => t.Outfit)
                .ThenInclude(o => o.User)
            .Include(t => t.Poll)
            .Where(t => t.Date >= cutoffDate)
            .OrderByDescending(t => t.TrendingScore)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrendingOutfit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.TrendingScore)
            .ToListAsync();
    }
}
