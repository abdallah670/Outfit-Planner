using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Query to get feed posts with cursor-based pagination
/// </summary>
public class GetFeedQuery : IRequest<CursorPagination.CursorPagedResult<FeedPostDto>>
{
    public string? UserId { get; set; }
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "popular";
    public string Visibility { get; set; } = "Public";
    public string? PostType { get; set; }
    public bool FollowingOnly { get; set; }
}

