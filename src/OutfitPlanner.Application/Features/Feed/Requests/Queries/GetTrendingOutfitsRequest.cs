using MediatR;
using OutfitPlanner.Application.DTOs.Social;

namespace OutfitPlanner.Application.Features.Social.Requests.Queries;

public class GetTrendingOutfitsRequest : IRequest<Responses.PagedResult<TrendingOutfitDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
