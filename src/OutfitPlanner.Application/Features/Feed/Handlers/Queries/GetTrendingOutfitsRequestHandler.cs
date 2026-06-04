using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

public class GetTrendingOutfitsRequestHandler : IRequestHandler<GetTrendingOutfitsRequest, CursorPagination.CursorPagedResult<TrendingOutfitDto>>
{
    private readonly ITrendingOutfitRepository _trendingOutfitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTrendingOutfitsRequestHandler(ITrendingOutfitRepository trendingOutfitRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _trendingOutfitRepository = trendingOutfitRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CursorPagination.CursorPagedResult<TrendingOutfitDto>> Handle(GetTrendingOutfitsRequest request, CancellationToken cancellationToken)
    {
        var result = await _trendingOutfitRepository.GetGlobalTrendingCursorAsync(request.Cursor, request.PageSize);
        
    
        var dtos = result.Items.Select(item =>
        {
            var dto = _mapper.Map<TrendingOutfitDto>(item);
            return dto;
        }).ToList();

        return new CursorPagination.CursorPagedResult<TrendingOutfitDto>
        {
            Items = dtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore,
            PageSize = result.PageSize
        };
    }

}
