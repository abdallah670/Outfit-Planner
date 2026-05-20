using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class CreateOutfitPostCommandHandler : IRequestHandler<CreateOutfitPostCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOutfitPostCommandHandler> _logger;

    public CreateOutfitPostCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateOutfitPostCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(CreateOutfitPostCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var outfit = await _unitOfWork.Outfits.GetByIdAsync(request.OutfitId, cancellationToken);
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
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.FeedPosts.AddAsync(feedPost, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Id = feedPost.Id;
            response.Success = true;
            response.Message = "Outfit post created successfully";

            _logger.LogInformation("Outfit post {PostId} for outfit {OutfitId} created by user {UserId}", 
                feedPost.Id, request.OutfitId, request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating outfit post for user {UserId}", request.UserId);
            response.Success = false;
            response.Message = "Error creating outfit post";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
