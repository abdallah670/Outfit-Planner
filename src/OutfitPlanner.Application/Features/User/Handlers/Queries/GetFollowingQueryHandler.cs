using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetFollowingQuery with cursor-based pagination
/// </summary>
public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, CursorPagination.CursorPagedResult<FollowingDto>>
{
    private readonly IFollowRepository _followRepository;

    public GetFollowingQueryHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<CursorPagination.CursorPagedResult<FollowingDto>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        var result = await _followRepository.GetFollowingCursorAsync(
            request.UserId,
            request.Cursor,
            request.PageSize);

        var dtos = result.Items.Select(f => new FollowingDto
        {
            UserId = f.FollowingId,
            UserName = f.Following?.UserName ?? "Unknown",
            AvatarUrl = f.Following?.ProfilePictureUrl,
            CreatedAt = f.CreatedAt.DateTime
        }).ToList();

        return new CursorPagination.CursorPagedResult<FollowingDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }
}
