using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

/// <summary>
/// Handler for GetFollowersQuery with cursor-based pagination
/// </summary>
public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, CursorPagination.CursorPagedResult<FollowerDto>>
{
    private readonly IFollowRepository _followRepository;

    public GetFollowersQueryHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<CursorPagination.CursorPagedResult<FollowerDto>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var result = await _followRepository.GetFollowersCursorAsync(
            request.UserId,
            request.Cursor,
            request.PageSize);

        var dtos = result.Items.Select(f => new FollowerDto
        {
            UserId = f.FollowerId,
            UserName = f.Follower?.UserName ?? "Unknown",
            AvatarUrl = f.Follower?.ProfilePictureUrl,
            CreatedAt = f.CreatedAt.DateTime
        }).ToList();

        return new CursorPagination.CursorPagedResult<FollowerDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }
}
