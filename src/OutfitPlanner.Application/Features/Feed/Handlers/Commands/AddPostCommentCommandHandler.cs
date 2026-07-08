using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class AddPostCommentCommandHandler : IRequestHandler<AddPostCommentCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IPostCommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ISocialHubService _socialHub;

    public AddPostCommentCommandHandler(
        IFeedPostRepository feedPostRepository,
        IPostCommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ISocialHubService socialHub)
    {
        _feedPostRepository = feedPostRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _socialHub = socialHub;
    }

    public async Task<BaseCommandResponse> Handle(AddPostCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var post = await _feedPostRepository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            response.Success = false;
            response.Message = "Post not found";
            return response;
        }

        var comment = new PostComment
        {
            PostId = request.PostId,
            UserId = request.UserId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _commentRepository.AddAsync(comment);

        post.CommentsCount++;
        await _feedPostRepository.UpdateAsync(post);

        await _unitOfWork.SaveChangesAsync();

        // Notify post owner (skip self-comments)
        if (post.UserId != request.UserId)
        {
            var url = post.PostType == PostType.Outfit ? $"/social/posts/{post.Id}" : $"/social/polls/{post.Id}";
            await _mediator.Send(new CreateNotificationCommand
            {
                UserId = post.UserId,
                Request = new CreateNotificationDto
                {
                    Type = OutfitPlanner.Domain.Enums.NotificationType.Social,
                    Title = $"Comment on \"{post.Caption ?? "your post"}\"",
                    Message = "Someone commented: \"" + request.Content + "\"",
                    ActionUrl = url
                }
            });
        }

        // Notify all connected users about the comment count update via SignalR
        try
        {
            await _socialHub.NotifyCommentUpdateAsync(request.PostId.ToString(), post.CommentsCount);
        }
        catch (Exception signalREx)
        {
            // Log but don't fail the operation
            System.Diagnostics.Debug.WriteLine($"SignalR comment update failed: {signalREx.Message}");
        }

        response.Success = true;
        response.Message = "Comment added successfully";
        return response;
    }
}