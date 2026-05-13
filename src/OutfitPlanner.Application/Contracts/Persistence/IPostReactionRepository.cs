using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IPostReactionRepository : IGenericRepository<PostReaction>
{
    Task<PostReaction?> GetReactionAsync(Guid postId, string userId);
    Task<bool> HasReactionAsync(Guid postId, string userId);
    Task<int> GetReactionCountAsync(Guid postId);
    Task<PostReaction> GetUserReaction(string userId, Guid id);
}
