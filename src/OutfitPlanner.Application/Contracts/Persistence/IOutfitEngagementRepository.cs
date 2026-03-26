using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IOutfitEngagementRepository
{
    /// <summary>
    /// Idempotent like: Returns true if newly liked, false if already liked or outfit not found
    /// </summary>
    Task<bool> LikeAsync(Guid outfitId, string userId);
    
    /// <summary>
    /// Idempotent unlike: Returns true if unliked, false if was not liked or outfit not found
    /// </summary>
    Task<bool> UnlikeAsync(Guid outfitId, string userId);
    
    Task<int> GetLikeCountAsync(Guid outfitId);
    Task<bool> HasUserLikedAsync(Guid outfitId, string userId);
    
    Task<OutfitComment> AddCommentAsync(OutfitComment comment);
    Task<PagedResult<OutfitComment>> GetCommentsAsync(Guid outfitId, int page, int pageSize);
    Task<int> GetCommentCountAsync(Guid outfitId);
    
    /// <summary>
    /// Soft deletes a comment. Returns false if comment not found or user is not the owner.
    /// </summary>
    Task<bool> SoftDeleteCommentAsync(Guid commentId, string userId);
    
    Task<OutfitComment?> GetCommentAsync(Guid commentId);
}
