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
        // Note: TrendingOutfit doesn't have a Location property, using Date instead
        return await _dbSet
            .Where(t => t.Date.Date == DateTime.Today)
            .OrderByDescending(t => t.TrendingScore)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrendingOutfit>> GetGlobalTrendingAsync(int count = 20)
    {
        return await _dbSet
            .OrderByDescending(t => t.TrendingScore)
            .Take(count)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(Guid outfitId)
    {
        var trending = await _dbSet.FirstOrDefaultAsync(t => t.OutfitId == outfitId);
        if (trending != null)
        {
            // Increment VoteCount as a proxy for view count
            trending.VoteCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TrendingOutfit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.TrendingScore)
            .ToListAsync();
    }
}
