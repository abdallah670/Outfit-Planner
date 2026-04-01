using MediatR;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

public class GetFollowersQuery : IRequest<FollowersResponse>
{
    public string UserId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
