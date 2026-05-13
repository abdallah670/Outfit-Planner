using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Queries;

public class GetTrendingOutfitsRequestHandler : IRequestHandler<GetTrendingOutfitsRequest, Responses.PagedResult<TrendingOutfitDto>>
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

    public async Task<Responses.PagedResult<TrendingOutfitDto>> Handle(GetTrendingOutfitsRequest request, CancellationToken cancellationToken)
    {
        var (trendingOutfits, totalCount) = await _trendingOutfitRepository.GetGlobalTrendingPagedAsync(request.Page, request.PageSize);
        var followedUserIds = (await _unitOfWork.Follows.FindAsync(f => f.FollowerId == request.UserId, cancellationToken))
                       .Select(f => f.FollowedId)
                       .ToList();
        // Get user's liked posts
        var likedPostIds = (await _unitOfWork.PostReactions.FindAsync(r => r.UserId == request.UserId, cancellationToken))
            .Select(r => r.PostId)
            .ToList();
       
        // Map and enrich posts with user context
        var dtos =new List<TrendingOutfitDto>().Select(post =>
        {
            var dto = _mapper.Map<TrendingOutfitDto>(post);
            dto.IsOwner = dto.UserId == request.UserId;

            // Check if user voted on this poll
       

            dto.IsFollowing = followedUserIds.Contains(dto.UserId);
            dto.IsLiked = likedPostIds.Contains(dto.Id);

            return dto;
        }).ToList();

        return new Responses.PagedResult<TrendingOutfitDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
