using MediatR;
using OutfitPlanner.Application.DTOs.Social;

namespace OutfitPlanner.Application.Features.Social.Requests.Queries;

public class GetLocalTrendsRequest : IRequest<TrendingDataDto>
{
}
