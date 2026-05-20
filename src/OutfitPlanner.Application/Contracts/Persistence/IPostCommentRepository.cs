using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IPostCommentRepository : IGenericRepository<PostComment>
{
    Task<(List<PostComment> Comments, int TotalCount)> GetCommentsAsync(Guid postId, int page, int pageSize, int maxDepth);
    Task<List<PostComment>> GetRootCommentsAsync(Guid postId, int page, int pageSize);
    Task<List<PostComment>> GetCommentsByPostIdAsync(Guid postId);
    Task<int> GetCommentsCountByPostIdAsync(Guid postId);
    /// <summary>
    /// Get root comments with cursor-based pagination
    /// </summary>
    Task<CursorPagination.CursorPagedResult<PostComment>> GetRootCommentsCursorAsync(Guid postId, string? cursor, int pageSize);
    Task<IEnumerable<PostComment>> GetByParentCommentId(Guid commentId);
}
