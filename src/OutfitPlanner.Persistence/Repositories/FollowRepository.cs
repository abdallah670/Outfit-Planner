using Microsoft.EntityFrameworkCore;
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

    public async Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        return await _dbSet
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<List<Follow>> GetFollowersAsync(string userId, int page, int pageSize)
    {
        return await _dbSet
            .Include(f => f.Follower)
            .Where(f => f.FollowingId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Follow>> GetFollowingAsync(string userId, int page, int pageSize)
    {
        return await _dbSet
            .Include(f => f.Following)
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowerId == userId);
    }
}
