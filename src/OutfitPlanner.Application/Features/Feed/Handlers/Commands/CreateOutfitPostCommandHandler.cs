using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class CreateOutfitPostCommandHandler : IRequestHandler<CreateOutfitPostCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IOutfitRepository _outfitRepository;

    public CreateOutfitPostCommandHandler(
        IFeedPostRepository feedPostRepository,
        IOutfitRepository outfitRepository)
    {
        _feedPostRepository = feedPostRepository;
        _outfitRepository = outfitRepository;
    }

    public async Task<BaseCommandResponse> Handle(CreateOutfitPostCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var outfit = await _outfitRepository.GetByIdAsync(request.OutfitId);
        if (outfit == null || outfit.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "Outfit not found or does not belong to user";
            return response;
        }

        var feedPost = new FeedPost
        {
            UserId = request.UserId,
            PostType = PostType.Outfit,
            OutfitId = request.OutfitId,
            Caption = request.Caption,
            Tags = request.Tags,
            Visibility = request.Visibility,
            LikesCount = 0,
            CommentsCount = 0,
            
        };

        await _feedPostRepository.AddAsync(feedPost);

        response.Id = feedPost.Id;
        response.Success = true;
        response.Message = "Outfit post created successfully";

        return response;
    }
}
