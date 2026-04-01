using MediatR;
using OutfitPlanner.Application.Common;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Query to get following with cursor-based pagination
/// </summary>
public class GetFollowingQuery : IRequest<CursorPagination.CursorPagedResult<FollowingDto>>
{
    public string UserId { get; set; } = string.Empty;
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO for a following user
/// </summary>
public class FollowingDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
