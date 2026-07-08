using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
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
    private readonly ISocialHubService _socialHub;

    public CreateOutfitPostCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateOutfitPostCommandHandler> logger,
        ISocialHubService socialHub)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _socialHub = socialHub;
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

            // Notify all connected users about the new post via SignalR
            try
            {
                var postDto = new
                {
                    id = feedPost.Id.ToString(),
                    userId = feedPost.UserId,
                    userName = outfit.Name ?? "Unknown",
                    userAvatarUrl = "",
                    caption = feedPost.Caption,
                    imageUrl = "",
                    createdAt = feedPost.CreatedAt.ToString("o"),
                    likesCount = feedPost.LikesCount,
                    commentsCount = feedPost.CommentsCount,
                    postType = "Outfit",
                    outfitId = feedPost.OutfitId?.ToString(),
                    pollId = (string?)null
                };
                await _socialHub.NotifyAllNewPostAsync(postDto);
                await _socialHub.NotifyNewPostAsync(feedPost.UserId, postDto);
            }
            catch (Exception signalREx)
            {
                _logger.LogWarning(signalREx, "Failed to send SignalR notification for new outfit post {PostId}", feedPost.Id);
            }
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
