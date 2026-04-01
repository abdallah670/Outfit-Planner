using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class PostCommentRepository : GenericRepository<PostComment>, IPostCommentRepository
{
    private readonly AppDbContext _context;

    public PostCommentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(List<PostComment> Comments, int TotalCount)> GetCommentsAsync(Guid postId, int page, int pageSize, int maxDepth)
    {
        var query = _dbSet
            .Include(c => c.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (comments, totalCount);
    }

    public async Task<List<PostComment>> GetRootCommentsAsync(Guid postId, int page, int pageSize)
    {
        return await _dbSet
            .Include(c => c.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
