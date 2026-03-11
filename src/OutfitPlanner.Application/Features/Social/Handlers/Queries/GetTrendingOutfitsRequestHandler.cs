using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Queries;

namespace OutfitPlanner.Application.Features.Social.Handlers.Queries;

public class GetTrendingOutfitsRequestHandler : IRequestHandler<GetTrendingOutfitsRequest, List<TrendingOutfitDto>>
{
    private readonly ITrendingOutfitRepository _trendingOutfitRepository;
    private readonly IMapper _mapper;

    public GetTrendingOutfitsRequestHandler(ITrendingOutfitRepository trendingOutfitRepository, IMapper mapper)
    {
        _trendingOutfitRepository = trendingOutfitRepository;
        _mapper = mapper;
    }

    public async Task<List<TrendingOutfitDto>> Handle(GetTrendingOutfitsRequest request, CancellationToken cancellationToken)
    {
        var trendingOutfits = await _trendingOutfitRepository.GetGlobalTrendingAsync(20);
        return _mapper.Map<List<TrendingOutfitDto>>(trendingOutfits);
    }
}
