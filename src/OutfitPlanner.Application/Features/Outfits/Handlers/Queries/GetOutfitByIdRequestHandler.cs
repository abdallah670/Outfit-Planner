using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Queries;

public class GetOutfitByIdRequestHandler : IRequestHandler<GetOutfitByIdRequest, OutfitDto>
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly ITrendingOutfitRepository _trendingOutfitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOutfitByIdRequestHandler> _logger;

    public GetOutfitByIdRequestHandler(
        IOutfitRepository outfitRepository, 
        IFeedPostRepository feedPostRepository,
        ITrendingOutfitRepository trendingOutfitRepository,
        IMapper mapper, 
        ILogger<GetOutfitByIdRequestHandler> logger)
    {
        _outfitRepository = outfitRepository;
        _feedPostRepository = feedPostRepository;
        _trendingOutfitRepository = trendingOutfitRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OutfitDto> Handle(GetOutfitByIdRequest request, CancellationToken cancellationToken)
    {
        try{
            var outfit=await _outfitRepository.GetWithItemsByIdAsync(request.Id);
            if(outfit==null){
                _logger.LogInformation("Outfit with ID {Id} not found", request.Id);
                throw new NotFoundException("Outfit", request.Id);
            }
            
            var outfitDto = _mapper.Map<OutfitDto>(outfit);
            
            // Map community feedback data
            var feedPost = await _feedPostRepository.GetFirstOrDefaultAsync(f => f.OutfitId == request.Id);
            if (feedPost != null)
            {
                outfitDto.FeedPostId = feedPost.Id;
                outfitDto.PostType = feedPost.PostType.ToString();
                outfitDto.LikesCount = feedPost.LikesCount;
                outfitDto.CommentsCount = feedPost.CommentsCount;
            }
            
            var trendingOutfit = await _trendingOutfitRepository.GetFirstOrDefaultAsync(t => t.OutfitId == request.Id);
            if (trendingOutfit != null)
            {
                outfitDto.Rank = trendingOutfit.RankPosition;
                outfitDto.Score =trendingOutfit.TrendingScore;
            }

            return outfitDto;

        }
        catch(NotFoundException){
            throw;
        }
        catch(Exception ex){
            _logger.LogError(ex, "Error getting outfit by ID");
            throw new BadRequestException("Error getting outfit by ID");
        }
    }
}
