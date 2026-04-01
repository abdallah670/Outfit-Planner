using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Query to get post comments with cursor-based pagination
/// </summary>
public class GetPostCommentsQuery : IRequest<CursorPagination.CursorPagedResult<PostCommentDto>>
{
    public Guid PostId { get; set; }
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
}
