using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Get feed posts for a specific user (for public profile activity tab)
/// </summary>
public class GetUserFeedQuery : IRequest<CursorPagination.CursorPagedResult<FeedPostDto>>
{
    public string UserId { get; set; } = string.Empty;
    public string? ViewerUserId { get; set; } // The currently logged-in user viewing the profile
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
    public string? PostType { get; set; } // "OutfitPost", "PollPost", or null for all
}
