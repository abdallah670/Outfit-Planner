using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class PostReactionRepository : GenericRepository<PostReaction>, IPostReactionRepository
{
    private readonly AppDbContext _context;

    public PostReactionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PostReaction?> GetReactionAsync(Guid postId, string userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
    }

    public async Task<bool> HasReactionAsync(Guid postId, string userId)
    {
        return await _dbSet
            .AnyAsync(r => r.PostId == postId && r.UserId == userId);
    }

    public async Task<int> GetReactionCountAsync(Guid postId)
    {
        return await _dbSet
            .CountAsync(r => r.PostId == postId);
    }
}
