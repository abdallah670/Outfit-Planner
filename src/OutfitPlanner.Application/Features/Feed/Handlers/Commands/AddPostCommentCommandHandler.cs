using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class AddPostCommentCommandHandler : IRequestHandler<AddPostCommentCommand, BaseCommandResponse>
{
    private readonly IPostCommentRepository _commentRepository;
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPostCommentCommandHandler(
        IPostCommentRepository commentRepository,
        IFeedPostRepository feedPostRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _feedPostRepository = feedPostRepository;
        _unitOfWork = unitOfWork;
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
            ParentCommentId = request.ParentCommentId
        };

        await _commentRepository.AddAsync(comment);

        // Update counts
        post.CommentsCount++;
        await _feedPostRepository.UpdateAsync(post);

        if (post.OutfitId.HasValue)
        {
            var outfit = await _unitOfWork.Repository<Outfit>().GetByIdAsync(post.OutfitId.Value);
            if (outfit != null)
            {
                outfit.CommentsCount++;
                await _unitOfWork.Repository<Outfit>().UpdateAsync(outfit);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        response.Success = true;
        response.Message = "Comment added successfully";
        response.Id = comment.Id;

        return response;
    }
}

public class DeletePostCommentCommandHandler : IRequestHandler<DeletePostCommentCommand, BaseCommandResponse>
{
    private readonly IPostCommentRepository _commentRepository;
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostCommentCommandHandler(
        IPostCommentRepository commentRepository,
        IFeedPostRepository feedPostRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _feedPostRepository = feedPostRepository;
        _unitOfWork = unitOfWork;
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
