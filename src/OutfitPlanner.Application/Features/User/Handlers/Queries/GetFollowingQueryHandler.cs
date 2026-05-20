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
            request.PageSize,
            request.SearchQuery);
        var followedUserIds = (await _followRepository.FindAsync(f => f.FollowerId == request.RequesterId, cancellationToken))
                      .Select(f => f.FollowedId)
                      .ToList();
        var dtos = result.Items.Select(f => new FollowingDto
        {
            UserId = f.FollowedId,
            UserName = f.Followed?.UserName ?? "Unknown",
            AvatarUrl = f.Followed?.ProfilePictureUrl,
            CreatedAt = f.CreatedAt.DateTime,
            IsFollowing = followedUserIds.Contains(f.FollowedId),
            IsOwner = f.FollowedId == request.RequesterId

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
