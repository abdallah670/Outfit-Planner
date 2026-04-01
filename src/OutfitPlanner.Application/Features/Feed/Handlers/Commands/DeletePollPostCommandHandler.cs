using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

/// <summary>
/// Handler for DeletePollPostCommand
/// </summary>
public class DeletePollPostCommandHandler : IRequestHandler<DeletePollPostCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePollPostCommandHandler(
        IFeedPostRepository feedPostRepository,
        IValidationPollRepository validationPollRepository,
        IUnitOfWork unitOfWork)
    {
        _feedPostRepository = feedPostRepository;
        _validationPollRepository = validationPollRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeletePollPostCommand request, CancellationToken cancellationToken)
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

        // Delete the poll first if exists
        if (post.PollId.HasValue)
        {
            var poll = await _validationPollRepository.GetByIdAsync(post.PollId.Value);
            if (poll != null)
            {
                await _validationPollRepository.RemoveAsync(poll);
            }
        }

        // Delete the feed post
        await _feedPostRepository.RemoveAsync(post);
        await _unitOfWork.SaveChangesAsync();

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Poll post deleted successfully"
        };
    }
}
