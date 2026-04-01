using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Query to get the most voted active poll with its comments (cursor-based pagination)
/// </summary>
public class GetRecentPollWithCommentsQuery : IRequest<RecentPollWithCommentsDto>
{
    public string? UserId { get; set; }
    public string? CommentsCursor { get; set; }
    public int CommentsPageSize { get; set; } = 20;
}
