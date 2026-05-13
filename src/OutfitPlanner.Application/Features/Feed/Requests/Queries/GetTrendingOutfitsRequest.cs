using MediatR;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

public class GetTrendingOutfitsRequest : IRequest<Responses.PagedResult<TrendingOutfitDto>>
{
    public int Page { get; set; } = 1;
    public string UserId { get; set; }
    public int PageSize { get; set; } = 20;
}
