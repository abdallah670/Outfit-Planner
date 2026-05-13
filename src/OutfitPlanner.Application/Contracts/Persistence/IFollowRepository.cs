using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IFollowRepository : IGenericRepository<Follow>
{
    Task<bool> IsFollowingAsync(string followerId, string followedId);
    Task<List<Follow>> GetFollowersAsync(string userId, int page, int pageSize);
    Task<List<Follow>> GetFollowingAsync(string userId, int page, int pageSize);
    Task<int> GetFollowersCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
    
    /// <summary>
    /// Get followers with cursor-based pagination
    /// </summary>
    Task<CursorPagination.CursorPagedResult<Follow>> GetFollowersCursorAsync(string userId, string? cursor, int pageSize);
    
    /// <summary>
    /// Get following with cursor-based pagination
    /// </summary>
    Task<CursorPagination.CursorPagedResult<Follow>> GetFollowingCursorAsync(string userId, string? cursor, int pageSize);
}
