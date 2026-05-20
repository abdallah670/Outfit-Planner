using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;
using OutfitPlanner.Application.Common;

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
                .ThenInclude(p => p.Options)
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
                .ThenInclude(p => p.Options)
            .Where(t => t.Date >= cutoffDate)
            .OrderByDescending(t => t.TrendingScore)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(IEnumerable<TrendingOutfit> Items, int TotalCount)> GetGlobalTrendingPagedAsync(int page, int pageSize)
    {
        var cutoffDate = DateTime.Today.AddDays(-7);
        
        // Get total count for pagination metadata
        var totalCount = await _dbSet
            .Where(t => t.Date >= cutoffDate)
            .CountAsync();
        
        // Get paginated items
        var items = await _dbSet
            .Include(t => t.Outfit)
                .ThenInclude(o => o.User)
            .Include(t => t.Poll)
                .ThenInclude(p => p.Options)
            .Where(t => t.Date >= cutoffDate)
            .OrderByDescending(t => t.TrendingScore)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<CursorPagination.CursorPagedResult<TrendingOutfit>> GetGlobalTrendingCursorAsync(string? cursor, int pageSize)
    {
        var cutoffDate = DateTime.Today.AddDays(-7);
        var query = _dbSet
            .Include(t => t.Outfit)
                .ThenInclude(o => o.User)
            .Include(t => t.Poll)
                .ThenInclude(p => p.Options)
            .Where(t => t.Date >= cutoffDate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(cursor))
        {
            var cursorData = CursorPagination.DecodeTrendingCursor(cursor);
            if (cursorData != null)
            {
                query = query.Where(t => t.TrendingScore < cursorData.Score || 
                                        (t.TrendingScore == cursorData.Score && t.Id.CompareTo(cursorData.Id) < 0));
            }
        }

        var items = await query
            .OrderByDescending(t => t.TrendingScore)
            .ThenByDescending(t => t.Id)
            .Take(pageSize + 1)
            .ToListAsync();

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(pageSize);

        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = CursorPagination.CreateTrendingCursor(lastItem.TrendingScore, lastItem.Id);
        }

        return new CursorPagination.CursorPagedResult<TrendingOutfit>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            PageSize = pageSize
        };
    }


    public async Task<IEnumerable<TrendingOutfit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.TrendingScore)
            .ToListAsync();
    }
}
