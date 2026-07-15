using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications;
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
            CreatedAt = DateTimeOffset.UtcNow,
            MentionedUsers = request.MentionedUsers?.Select(m=>m.UserId).ToList() ?? new List<string>()
        };

        await _commentRepository.AddAsync(comment);

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
            if (parentComment != null)
            {
                parentComment.TotalReplies++;
                await _commentRepository.UpdateAsync(parentComment);

                // First mention is always the parent comment's author (reply rule).
                if (!comment.MentionedUsers.Any(m => m== parentComment.UserId))
                {
                    var parentUser = await _unitOfWork.Users.GetByIdAsync(parentComment.UserId);
                    comment.MentionedUsers.Add(
                        parentComment.UserId
                    );
                }
                
            }
        }

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

        // Notify users mentioned directly in the comment, plus users tagged in the post.
        var mentionedIds = comment.MentionedUsers 
            .Where(id => !string.IsNullOrWhiteSpace(id) && id != request.UserId)
            .Distinct()
            .ToList();

        if (post.Tags.Any())
        {
            var taggedUsers = await _unitOfWork.Users.GetTaggedUsersAsync(post.Tags);
            mentionedIds = mentionedIds
                .Union(taggedUsers.Select(t => t.UserId))
                .Where(id => id != request.UserId && id != post.UserId)
                .Distinct()
                .ToList();
        }
        else
        {
            mentionedIds = mentionedIds.Where(id => id != post.UserId).Distinct().ToList();
        }

        if (mentionedIds.Any())
        {
            var mentionUrl = post.PostType == PostType.Outfit ? $"/social/posts/{post.Id}" : $"/social/polls/{post.Id}";
            await _mediator.NotifyMentionedUsersAsync(
                mentionedIds,
                request.UserId,
                request.Content,
                mentionUrl,
                title: "New mention",
                verb: "mentioned");
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
public class DeletePostCommentCommandHandler : IRequestHandler<DeletePostCommentCommand, BaseCommandResponse>
{
    private readonly IPostCommentRepository _commentRepository;
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;
  
    private readonly IMediator _mediator;
    private readonly ISocialHubService _socialHub;
    public DeletePostCommentCommandHandler(
        IPostCommentRepository commentRepository,
        IFeedPostRepository feedPostRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ISocialHubService socialHub)
    {
        _commentRepository = commentRepository;
        _feedPostRepository = feedPostRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _socialHub = socialHub;
    }

    public async Task<BaseCommandResponse> Handle(DeletePostCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        
        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment == null)
        {
            response.Success = false;
            response.Message = "Comment not found";
            return response;
        }

        if (comment.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "Unauthorized to delete this comment";
            return response;
        }
        //first change the replies parent comment id to removed comment parent comment id

        var commentReplies = await _commentRepository.GetByParentCommentId(request.CommentId);
        if (commentReplies != null)
        {
            foreach (var commentReply in commentReplies)
            {
                commentReply.ParentCommentId = comment.ParentCommentId;
                await _commentRepository.UpdateAsync(commentReply);
            }
        }

        await _commentRepository.RemoveAsync(comment);

        var post = await _feedPostRepository.GetByIdAsync(comment.PostId);
        if (post != null)
        {
            if (post.CommentsCount > 0)
            {
                post.CommentsCount--;
                await _feedPostRepository.UpdateAsync(post);
            }

            if (post.OutfitId.HasValue)
            {
                var outfit = await _unitOfWork.Repository<Outfit>().GetByIdAsync(post.OutfitId.Value);
                if (outfit != null && outfit.CommentsCount > 0)
                {
                    outfit.CommentsCount--;
                    await _unitOfWork.Repository<Outfit>().UpdateAsync(outfit);


                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

    // Notify all connected users about the comment count update via SignalR
        try
        {
            await _socialHub.NotifyCommentUpdateAsync(comment.PostId.ToString(), post.CommentsCount);
        }
        catch (Exception signalREx)
        {
            // Log but don't fail the operation
            System.Diagnostics.Debug.WriteLine($"SignalR comment update failed: {signalREx.Message}");
        }
        response.Success = true;
        response.Message = "Comment deleted successfully";

        return response;
    }
}

public class UpdatePostCommentCommandHandler : IRequestHandler<UpdatePostCommentCommand, BaseCommandResponse>
{
    private readonly IPostCommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostCommentCommandHandler(
        IPostCommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdatePostCommentCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment == null)
        {
            response.Success = false;
            response.Message = "Comment not found";
            return response;
        }

        if (comment.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "Unauthorized to update this comment";
            return response;
        }

        comment.Content = request.Content;
        await _commentRepository.UpdateAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        response.Success = true;
        response.Message = "Comment updated successfully";

        return response;
    }
}