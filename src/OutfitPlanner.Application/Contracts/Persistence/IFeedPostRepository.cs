using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IFeedPostRepository : IGenericRepository<FeedPost>
{
    /// <summary>
    /// Get feed posts with cursor-based pagination
    /// </summary>
    Task<CursorPagination.CursorPagedResult<FeedPost>> GetFeedAsync(
        string? userId, 
        string? cursor, 
        int pageSize, 
        string sortBy, 
        Visibility visibility,
        PostType? postType);
    
    Task<FeedPost?> GetByIdWithDetailsAsync(Guid id);
    Task<FeedPost?> GetByOutfitIdAsync(Guid outfitId);
    Task<FeedPost?> GetByPollIdAsync(Guid pollId);
    Task<List<FeedPost>> GetUserPostsAsync(string userId, int page, int pageSize);
    
}
