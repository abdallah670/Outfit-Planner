using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

/// <summary>
/// Handler for DeleteFeedPostCommand
/// </summary>
public class DeleteFeedPostCommandHandler : IRequestHandler<DeleteFeedPostCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFeedPostCommandHandler(
        IFeedPostRepository feedPostRepository,
        IUnitOfWork unitOfWork)
    {
        _feedPostRepository = feedPostRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeleteFeedPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _feedPostRepository.GetByIdAsync(request.PostId);

        if (post == null)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "Post not found"
            };
        }

        // Verify ownership
        if (post.UserId != request.UserId)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "You can only delete your own posts"
            };
        }

        await _feedPostRepository.RemoveAsync(post);
        await _unitOfWork.SaveChangesAsync();

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Post deleted successfully"
        };
    }
}
