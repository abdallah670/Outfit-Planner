using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Persistence.Repositories;

public class FeedPostRepository : GenericRepository<FeedPost>, IFeedPostRepository
{
    private readonly AppDbContext _context;

    public FeedPostRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CursorPagination.CursorPagedResult<FeedPost>> GetFeedAsync(
        string? userId, 
        string? cursor, 
        int pageSize, 
        string? sortBy, 
        Visibility visibility,
        PostType? postType,
        bool followingOnly = false)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Outfit)
            .Include(p => p.Poll)
                .ThenInclude(p => p!.Options)
                    .ThenInclude(o => o.Outfit)
            .Include(p => p.Poll)
                .ThenInclude(p => p!.Options)
                    .ThenInclude(o => o.Votes)
            .Include(p => p.Reactions)
            .AsQueryable();

        if (followingOnly && !string.IsNullOrEmpty(userId))
        {
            var followedUserIds = _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowedId);
                
            query = query.Where(p => followedUserIds.Contains(p.UserId) && 
                (p.Visibility == Visibility.Public || p.Visibility == Visibility.Followers));
        }
        else
        {
            // Global Feed (All Tab)
            if (!string.IsNullOrEmpty(userId))
            {
                // If viewer is provided, show:
                // 1. All Public posts
                // 2. Followers posts from users the viewer follows
                // 3. The viewer's own posts (any visibility)
                var followedUserIds = _context.Follows
                    .Where(f => f.FollowerId == userId)
                    .Select(f => f.FollowedId);

                query = query.Where(p => p.Visibility == Visibility.Public || 
                                        (p.Visibility == Visibility.Followers && followedUserIds.Contains(p.UserId)) ||
                                        p.UserId == userId);
            }
            else
            {
                // Anonymous viewer: only Public posts
                query = query.Where(p => p.Visibility == Visibility.Public);
            }
        }




        if (postType.HasValue)
        {
            query = query.Where(p => p.PostType == postType.Value);
        }
        




        // Apply cursor filter if provided
        if (!string.IsNullOrEmpty(cursor))
        {
            var cursorData = CursorPagination.DecodeCursor(cursor);
            if (cursorData != null)
            {
                query = query.Where(p => p.CreatedAt < cursorData.CreatedAt || 
                                        (p.CreatedAt == cursorData.CreatedAt && p.Id.CompareTo(cursorData.Id) < 0));
            }
        }

        // Order by CreatedAt descending (consistent with cursor)
        var orderedQuery = query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

        // Take one extra to check if there's more
        var posts = await orderedQuery
            .Take(pageSize + 1)
            .ToListAsync();

        // Check if there's more data
        var hasMore = posts.Count > pageSize;
        var items = hasMore ? posts.Take(pageSize).ToList() : posts;

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = CursorPagination.CreateCursor(lastItem.CreatedAt, lastItem.Id);
        }

        return new CursorPagination.CursorPagedResult<FeedPost>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            PageSize = pageSize
        };
    }

    public async Task<FeedPost?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Outfit)
            .Include(p => p.Poll)
                .ThenInclude(p => p!.Options)
                    .ThenInclude(o => o.Outfit)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Reactions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<FeedPost?> GetByOutfitIdAsync(Guid outfitId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.OutfitId == outfitId);
    }

    public async Task<FeedPost?> GetByPollIdAsync(Guid pollId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PollId == pollId);
    }

    public async Task<List<FeedPost>> GetUserPostsAsync(string userId, int page, int pageSize)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Outfit)
            .Include(p => p.Poll)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }


}
