using Microsoft.EntityFrameworkCore;
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

    public async Task<(List<FeedPost> Posts, int TotalCount)> GetFeedAsync(string? userId, int page, int pageSize, string sortBy, Visibility visibility)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Outfit)
            .Include(p => p.Poll)
                .ThenInclude(p => p!.Options)
            .Where(p => p.Visibility == visibility)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var orderedQuery = sortBy.ToLower() == "popular"
            ? query.OrderByDescending(p => p.LikeCount).ThenByDescending(p => p.CreatedAt)
            : query.OrderByDescending(p => p.CreatedAt);

        var posts = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<FeedPost?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Outfit)
            .Include(p => p.Poll)
                .ThenInclude(p => p!.Options)
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
