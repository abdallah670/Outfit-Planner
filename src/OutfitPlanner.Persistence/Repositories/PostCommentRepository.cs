using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common;
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

    public async Task<CursorPagination.CursorPagedResult<PostComment>> GetRootCommentsCursorAsync(Guid postId, string? cursor, int pageSize)
    {
        var query = _dbSet
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                    .ThenInclude(rr => rr.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                    .ThenInclude(rr => rr.Replies)
                        .ThenInclude(rrr => rrr.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
            .AsQueryable();

        // Apply cursor filter if provided
        if (!string.IsNullOrEmpty(cursor))
        {
            var cursorData = CursorPagination.DecodeCursor(cursor);
            if (cursorData != null)
            {
                query = query.Where(c => c.CreatedAt < cursorData.CreatedAt || 
                                        (c.CreatedAt == cursorData.CreatedAt && c.Id.CompareTo(cursorData.Id) < 0));
            }
        }

        // Order by CreatedAt descending (consistent with cursor)
        var orderedQuery = query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id);

        // Take one extra to check if there's more
        var comments = await orderedQuery
            .Take(pageSize + 1)
            .ToListAsync();

        // Check if there's more data
        var hasMore = comments.Count > pageSize;
        var items = hasMore ? comments.Take(pageSize).ToList() : comments;

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = CursorPagination.CreateCursor(lastItem.CreatedAt, lastItem.Id);
        }

        return new CursorPagination.CursorPagedResult<PostComment>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            PageSize = pageSize
        };
    }

    public async Task<List<PostComment>> GetCommentsByPostIdAsync(Guid postId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> GetCommentsCountByPostIdAsync(Guid postId)
    {
        return await _dbSet
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .CountAsync();
    }
    public async Task<IEnumerable<PostComment>> GetByParentCommentId(Guid commentId)
    {
        //get replies of the comment with the parent comment id
        return await _dbSet
            .Include(c => c.Replies)
            .Where(c => c.ParentCommentId == commentId && !c.IsDeleted)
            .ToListAsync();
    }
}
