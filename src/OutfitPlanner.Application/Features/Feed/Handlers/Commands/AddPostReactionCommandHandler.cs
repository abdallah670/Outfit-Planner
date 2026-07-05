using MediatR;
using Microsoft.VisualBasic;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class AddPostReactionCommandHandler : IRequestHandler<AddPostReactionCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IPostReactionRepository _reactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public AddPostReactionCommandHandler(
        IFeedPostRepository feedPostRepository,
        IPostReactionRepository reactionRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _feedPostRepository = feedPostRepository;
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<BaseCommandResponse> Handle(AddPostReactionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var post = await _feedPostRepository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            response.Success = false;
            response.Message = "Post not found";
            return response;
        }

        var existingReaction = await _reactionRepository.GetReactionAsync(request.PostId, request.UserId);
        if (existingReaction != null)
        {
            if (existingReaction.IsDeleted)
            {
                existingReaction.IsDeleted=false;
                existingReaction.UpdatedAt=DateTime.UtcNow;
                await _reactionRepository.UpdateAsync(existingReaction);
                await _unitOfWork.SaveChangesAsync();
                
                response.Success = true;
                response.Message = "Reaction restored successfully";
                return response;
            }
            response.Success = false;
            response.Message = "You have already reacted to this post";
            return response;
        }

        var reaction = new PostReaction
        {
            PostId = request.PostId,
            UserId = request.UserId,
            ReactionType = (ReactionType)Enum.Parse(typeof(ReactionType), request.ReactionType)
        };

        await _reactionRepository.AddAsync(reaction);

        post.LikesCount++;
        await _feedPostRepository.UpdateAsync(post);

        // Update Outfit if linked
        if (post.OutfitId.HasValue)
        {
            var outfit = await _unitOfWork.Repository<Outfit>().GetByIdAsync(post.OutfitId.Value);
            if (outfit != null)
            {
                outfit.LikesCount++;
                await _unitOfWork.Repository<Outfit>().UpdateAsync(outfit);
            }
        }
        await _unitOfWork.SaveChangesAsync();

        if (post.UserId != request.UserId)
        {
            var url = post.PostType == PostType.Outfit ? $"/social/posts/{post.Id}" : $"/social/polls/{post.Id}";
            await _mediator.Send(new CreateNotificationCommand
            {
                
                UserId = post.UserId,
                Request = new CreateNotificationDto
                {
                    Type = OutfitPlanner.Domain.Enums.NotificationType.Social,
                    Title = "New like on your outfit",
                    Message = "Someone reacted to your post.",
                    ActionUrl = url
                }
            });
        }

        response.Success = true;
        response.Message = "Reaction added successfully";
        response.Data = new { postOwnerId = post.UserId, likesCount = post.LikesCount };

        return response;
    }
}

public class RemovePostReactionCommandHandler : IRequestHandler<RemovePostReactionCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IPostReactionRepository _reactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePostReactionCommandHandler(
        IFeedPostRepository feedPostRepository,
        IPostReactionRepository reactionRepository,
        IUnitOfWork unitOfWork)
    {
        _feedPostRepository = feedPostRepository;
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(RemovePostReactionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var reaction = await _reactionRepository.GetReactionAsync(request.PostId, request.UserId);
        if (reaction == null)
        {
            response.Success = false;
            response.Message = "Reaction not found";
            return response;
        }

        await _reactionRepository.RemoveAsync(reaction);

        var post = await _feedPostRepository.GetByIdAsync(request.PostId);
        if (post != null)
        {
            if (post.LikesCount > 0)
            {
                post.LikesCount--;
                await _feedPostRepository.UpdateAsync(post);
            }

            // Update Outfit if linked
            if (post.OutfitId.HasValue)
            {
                var outfit = await _unitOfWork.Repository<Outfit>().GetByIdAsync(post.OutfitId.Value);
                if (outfit != null && outfit.LikesCount > 0)
                {
                    outfit.LikesCount--;
                    await _unitOfWork.Repository<Outfit>().UpdateAsync(outfit);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        response.Success = true;
        response.Message = "Reaction removed successfully";
        response.Data = new { postOwnerId = post?.UserId, likesCount = post?.LikesCount };

        return response;
    }
}