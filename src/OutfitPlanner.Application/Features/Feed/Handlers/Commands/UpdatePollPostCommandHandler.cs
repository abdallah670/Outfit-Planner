using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using Microsoft.EntityFrameworkCore;

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
            // Fallback: search by PollId because the controller passes pollId in request.PostId
            post = await _feedPostRepository.GetByPollIdAsync(request.PostId);
        }

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
        if (request.Tags != null)
        {
            post.Tags = request.Tags;
        }
        await _feedPostRepository.UpdateAsync(post);

        // Update Poll fields if poll exists
        if (post.PollId.HasValue)
        {
            var poll = await _validationPollRepository.GetFirstOrDefaultAsync(
                p => p.Id == post.PollId.Value,
                query => query.Include(p => p.Options)
            );
            
            if (poll != null)
            {
                if (!string.IsNullOrEmpty(request.Question))
                    poll.Question = request.Question;
                
                if (request.ExpiresAt.HasValue)
                    poll.ExpiresAt = request.ExpiresAt.Value;

                if (!string.IsNullOrEmpty(request.Context))
                    poll.Context = request.Context;

                if (request.Options != null && request.Options.Any())
                {
                    foreach (var optionDto in request.Options)
                    {
                        var existingOption = poll.Options.FirstOrDefault(o => o.Id == optionDto.Id);
                        if (existingOption != null)
                        {
                            existingOption.Description = optionDto.Description ?? existingOption.Description;
                            await _unitOfWork.PollOptions.UpdateAsync(existingOption);
                        }
                    }
                }

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
