using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

/// <summary>
/// Handler for UpdatePollPostCommand
/// </summary>
public class UpdatePollPostCommandHandler : IRequestHandler<UpdatePollPostCommand, BaseCommandResponse>
{
    private readonly IFeedPostRepository _feedPostRepository;
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePollPostCommandHandler(
        IFeedPostRepository feedPostRepository,
        IValidationPollRepository validationPollRepository,
        IUnitOfWork unitOfWork)
    {
        _feedPostRepository = feedPostRepository;
        _validationPollRepository = validationPollRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdatePollPostCommand request, CancellationToken cancellationToken)
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
                Message = "You can only update your own posts"
            };
        }

        // Update FeedPost fields
        post.Visibility = request.Visibility;
        post.UpdatedAt = DateTimeOffset.UtcNow;
        await _feedPostRepository.UpdateAsync(post);

        // Update Poll fields if poll exists
        if (post.PollId.HasValue)
        {
            var poll = await _validationPollRepository.GetByIdAsync(post.PollId.Value);
            if (poll != null)
            {
                if (!string.IsNullOrEmpty(request.Question))
                    poll.Question = request.Question;
                
                if (request.ExpiresAt.HasValue)
                    poll.ExpiresAt = request.ExpiresAt.Value;

                await _validationPollRepository.UpdateAsync(poll);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return new BaseCommandResponse
        {
            Success = true,
            Id = post.Id,
            Message = "Poll post updated successfully"
        };
    }
}
