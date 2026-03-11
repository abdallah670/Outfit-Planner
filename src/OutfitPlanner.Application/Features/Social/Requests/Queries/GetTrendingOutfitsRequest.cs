using MediatR;
using OutfitPlanner.Application.DTOs.Social;

namespace OutfitPlanner.Application.Features.Social.Requests.Queries;

public class GetTrendingOutfitsRequest : IRequest<List<TrendingOutfitDto>>
{
}
