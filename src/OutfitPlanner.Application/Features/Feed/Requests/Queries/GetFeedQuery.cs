using MediatR;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

public class GetFeedQuery : IRequest<FeedPostResponse>
{
    public string? UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "popular";
    public Visibility Visibility { get; set; } = Visibility.Public;
}
