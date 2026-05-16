using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Query to get followers with cursor-based pagination
/// </summary>
public class GetFollowersQuery : IRequest<CursorPagination.CursorPagedResult<FollowerDto>>
{
    public string UserId { get; set; } = string.Empty;

    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
    public string? RequesterId { get; set; }
    public string? SearchQuery { get; set; }
}

/// <summary>
/// DTO for a follower
/// </summary>
public class FollowerDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFollowing { get; set; } = false;
    public bool IsOwner { get; set; } = false;

}
