using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

public class GetTrendingOutfitsRequestHandler : IRequestHandler<GetTrendingOutfitsRequest, Responses.PagedResult<TrendingOutfitDto>>
{
    private readonly ITrendingOutfitRepository _trendingOutfitRepository;
    private readonly IMapper _mapper;

    public GetTrendingOutfitsRequestHandler(ITrendingOutfitRepository trendingOutfitRepository, IMapper mapper)
    {
        _trendingOutfitRepository = trendingOutfitRepository;
        _mapper = mapper;
    }

    public async Task<Responses.PagedResult<TrendingOutfitDto>> Handle(GetTrendingOutfitsRequest request, CancellationToken cancellationToken)
    {
        var (trendingOutfits, totalCount) = await _trendingOutfitRepository.GetGlobalTrendingPagedAsync(request.Page, request.PageSize);
        
        return new Responses.PagedResult<TrendingOutfitDto>
        {
            Items = _mapper.Map<List<TrendingOutfitDto>>(trendingOutfits),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
