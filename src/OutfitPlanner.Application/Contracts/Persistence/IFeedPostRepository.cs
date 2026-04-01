using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IFeedPostRepository : IGenericRepository<FeedPost>
{
    Task<(List<FeedPost> Posts, int TotalCount)> GetFeedAsync(string? userId, int page, int pageSize, string sortBy, Visibility visibility);
    Task<FeedPost?> GetByIdWithDetailsAsync(Guid id);
    Task<FeedPost?> GetByOutfitIdAsync(Guid outfitId);
    Task<FeedPost?> GetByPollIdAsync(Guid pollId);
    Task<List<FeedPost>> GetUserPostsAsync(string userId, int page, int pageSize);
}
