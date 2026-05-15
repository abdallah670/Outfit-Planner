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
        
        var followedUserIds = (await _unitOfWork.Follows.FindAsync(f => f.FollowerId == request.UserId, cancellationToken))
                       .Select(f => f.FollowedId)
                       .ToList();
                       
        // Get user's liked posts
        var likedPostIds = (await _unitOfWork.PostReactions.FindAsync(r => r.UserId == request.UserId, cancellationToken))
            .Select(r => r.PostId)
            .ToList();
       
        // Map and enrich posts with user context
        var dtos = result.Items.Select(item =>
        {
            var dto = _mapper.Map<TrendingOutfitDto>(item);
            dto.IsOwner = dto.UserId == request.UserId;
            dto.IsFollowing = followedUserIds.Contains(dto.UserId);
            dto.IsLiked = likedPostIds.Contains(dto.Id);
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
