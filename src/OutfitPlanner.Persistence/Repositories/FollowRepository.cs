using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class FollowRepository : GenericRepository<Follow>, IFollowRepository
{
    private readonly AppDbContext _context;

    public FollowRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followedId)
    {
      var follow = await _dbSet
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        return follow != null;
    }

    public async Task<List<Follow>> GetFollowersAsync(string userId, int page, int pageSize)
    {
        return await _dbSet
            .Include(f => f.Follower)
            .Where(f => f.FollowedId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Follow>> GetFollowingAsync(string userId, int page, int pageSize)
    {
        return await _dbSet
            .Include(f => f.Followed)
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowedId == userId);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowerId == userId);
    }

    public async Task<CursorPagination.CursorPagedResult<Follow>> GetFollowersCursorAsync(string userId, string? cursor, int pageSize)
    {
        var query = _dbSet
            .Include(f => f.Follower)
            .Where(f => f.FollowedId == userId)
            .AsQueryable();

        // Apply cursor filter if provided
        if (!string.IsNullOrEmpty(cursor))
        {
            var cursorData = CursorPagination.DecodeCursor(cursor);
            if (cursorData != null)
            {
                query = query.Where(f => f.CreatedAt < cursorData.CreatedAt || 
                                        (f.CreatedAt == cursorData.CreatedAt && f.Id.CompareTo(cursorData.Id) < 0));
            }
        }

        // Order by CreatedAt descending
        var orderedQuery = query.OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.Id);

        // Take one extra to check if there's more
        var follows = await orderedQuery
            .Take(pageSize + 1)
            .ToListAsync();

        // Check if there's more data
        var hasMore = follows.Count > pageSize;
        var items = hasMore ? follows.Take(pageSize).ToList() : follows;

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = CursorPagination.CreateCursor(lastItem.CreatedAt, lastItem.Id);
        }

        return new CursorPagination.CursorPagedResult<Follow>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            PageSize = pageSize
        };
    }

    public async Task<CursorPagination.CursorPagedResult<Follow>> GetFollowingCursorAsync(string userId, string? cursor, int pageSize)
    {
        var query = _dbSet
            .Include(f => f.Followed)
            .Where(f => f.FollowerId == userId)
            .AsQueryable();

        // Apply cursor filter if provided
        if (!string.IsNullOrEmpty(cursor))
        {
            var cursorData = CursorPagination.DecodeCursor(cursor);
            if (cursorData != null)
            {
                query = query.Where(f => f.CreatedAt < cursorData.CreatedAt || 
                                        (f.CreatedAt == cursorData.CreatedAt && f.Id.CompareTo(cursorData.Id) < 0));
            }
        }

        // Order by CreatedAt descending
        var orderedQuery = query.OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.Id);

        // Take one extra to check if there's more
        var follows = await orderedQuery
            .Take(pageSize + 1)
            .ToListAsync();

        // Check if there's more data
        var hasMore = follows.Count > pageSize;
        var items = hasMore ? follows.Take(pageSize).ToList() : follows;

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = CursorPagination.CreateCursor(lastItem.CreatedAt, lastItem.Id);
        }

        return new CursorPagination.CursorPagedResult<Follow>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            PageSize = pageSize
        };
    }
}
