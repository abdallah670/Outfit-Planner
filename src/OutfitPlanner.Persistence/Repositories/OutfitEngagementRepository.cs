using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class OutfitEngagementRepository : IOutfitEngagementRepository
{
    private readonly AppDbContext _context;

    public OutfitEngagementRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> LikeAsync(Guid outfitId, string userId)
    {
        // First check if already liked to avoid exception in happy path
        if (await HasUserLikedAsync(outfitId, userId))
            return false;

        try
        {
            var like = new OutfitLike
            {
                OutfitId = outfitId,
                UserId = userId
            };
            
            _context.OutfitLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Race condition: another request from same user already inserted the like
            return false; 
        }
    }

    public async Task<bool> UnlikeAsync(Guid outfitId, string userId)
    {
        var like = await _context.OutfitLikes
            .FirstOrDefaultAsync(l => l.OutfitId == outfitId && l.UserId == userId);

        if (like == null)
            return false;

        _context.OutfitLikes.Remove(like);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetLikeCountAsync(Guid outfitId)
    {
        return await _context.OutfitLikes.CountAsync(l => l.OutfitId == outfitId);
    }

    public async Task<bool> HasUserLikedAsync(Guid outfitId, string userId)
    {
        return await _context.OutfitLikes.AnyAsync(l => l.OutfitId == outfitId && l.UserId == userId);
    }

    public async Task<OutfitComment> AddCommentAsync(OutfitComment comment)
    {
        _context.OutfitComments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Load the user navigation property for returning
        await _context.Entry(comment).Reference(c => c.User).LoadAsync();
        return comment;
    }

    public async Task<PagedResult<OutfitComment>> GetCommentsAsync(Guid outfitId, int page, int pageSize)
    {
        var query = _context.OutfitComments
            .Include(c => c.User)
            .Where(c => c.OutfitId == outfitId && !c.IsDeleted && c.ParentCommentId == null) // Top-level comments only
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        
        var comments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OutfitComment>
        {
            Items = comments,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetCommentCountAsync(Guid outfitId)
    {
        return await _context.OutfitComments.CountAsync(c => c.OutfitId == outfitId && !c.IsDeleted);
    }

    public async Task<bool> SoftDeleteCommentAsync(Guid commentId, string userId)
    {
        var comment = await _context.OutfitComments
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null || comment.UserId != userId)
            return false;

        comment.IsDeleted = true;
        // Optionally anonymize content
        comment.Content = "[This comment has been deleted]";
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OutfitComment?> GetCommentAsync(Guid commentId)
    {
        return await _context.OutfitComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("UNIQUE") == true
            || ex.InnerException?.Message.Contains("duplicate") == true
            || ex.InnerException?.Message.Contains("IX_") == true;
    }
}
