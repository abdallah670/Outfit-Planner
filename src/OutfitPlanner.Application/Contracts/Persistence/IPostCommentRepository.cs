using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IPostCommentRepository : IGenericRepository<PostComment>
{
    Task<(List<PostComment> Comments, int TotalCount)> GetCommentsAsync(Guid postId, int page, int pageSize, int maxDepth);
    Task<List<PostComment>> GetRootCommentsAsync(Guid postId, int page, int pageSize);
}
