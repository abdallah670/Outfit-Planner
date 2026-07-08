using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.DTOs.Notification;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

public class VoteOnPollCommandHandler : IRequestHandler<VoteOnPollCommand, BaseCommandResponse>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IPollOptionRepository _pollOptionRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VoteOnPollCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly ISocialHubService _socialHub;

    public VoteOnPollCommandHandler(
        IValidationPollRepository validationPollRepository,
        IPollOptionRepository pollOptionRepository,
        IVoteRepository voteRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<VoteOnPollCommandHandler> logger,
        IMediator mediator,
        ISocialHubService socialHub)
    {
        _validationPollRepository = validationPollRepository;
        _pollOptionRepository = pollOptionRepository;
        _voteRepository = voteRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _mediator = mediator;
        _socialHub = socialHub;
    }

    public async Task<BaseCommandResponse> Handle(VoteOnPollCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        try
        {
            var poll = await _validationPollRepository.GetByIdAsync(request.PollId);
            if (poll == null)
            {
                response.Success = false; response.Message = "Poll not found"; response.Errors.Add("Poll not found"); return response;
            }
            if (poll.Status != PollStatus.Active)
            {
                response.Success = false; response.Message = "Poll is not active"; response.Errors.Add("Poll is not active"); return response;
            }
            if (poll.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                response.Success = false; response.Message = "Poll has expired"; response.Errors.Add("Poll has expired"); return response;
            }

            var option = await _pollOptionRepository.GetByIdAsync(request.Request.OptionId);
            if (option == null || option.PollId != request.PollId)
            {
                response.Success = false; response.Message = "Option not found in this poll"; response.Errors.Add("Option not found in this poll"); return response;
            }

            var feedPost = await _unitOfWork.FeedPosts.GetByPollIdAsync(request.PollId);
            if (feedPost == null)
            {
                response.Success = false; response.Message = "Feed post not found"; response.Errors.Add("Feed post not found"); return response;
            }

            // --- Step 1: Handle the user's existing active vote (switching) ---
            var activeVote = await _voteRepository.GetUserVote(request.UserId, request.PollId);
            if (activeVote != null)
            {
                if (activeVote.OptionId == request.Request.OptionId)
                {
                    response.Success = false; response.Message = "You have already voted for this option"; response.Errors.Add("You have already voted for this option"); return response;
                }
                // Switching to a different option — soft-delete the current vote
                await _voteRepository.RemoveAsync(activeVote);
            }

            // --- Step 2: Check if a soft-deleted vote exists for the target option (switching back) ---
            var existingDeletedVote = await _voteRepository.GetUserVoteWithDeletedByOptionIdAsync(request.UserId, request.Request.OptionId);
            Guid voteId;
            if (existingDeletedVote != null && existingDeletedVote.IsDeleted)
            {
                // Restore the previously soft-deleted vote to avoid unique index violation
                existingDeletedVote.IsDeleted = false;
                existingDeletedVote.DeletedAt = null;
                existingDeletedVote.UpdatedAt = DateTimeOffset.UtcNow;
                await _voteRepository.UpdateAsync(existingDeletedVote);
                voteId = existingDeletedVote.Id;
            }
            else
            {
                // Brand new vote
                var newVote = new Vote { PollId = request.PollId, OptionId = request.Request.OptionId, VoterId = request.UserId };
                await _voteRepository.AddAsync(newVote);
                voteId = newVote.Id;

                // Only add a reaction if the user doesn't already have one (first-time voter)
                if (activeVote == null)
                {
                    var existingReaction = await _unitOfWork.PostReactions.GetUserReaction(request.UserId, feedPost.Id);
                    if (existingReaction == null)
                    {
                        await _unitOfWork.PostReactions.AddAsync(new PostReaction
                        {
                            PostId = feedPost.Id,
                            UserId = request.UserId,
                            ReactionType = ReactionType.Heart
                        });
                    }
                }
            }

            // --- Step 3: Save all vote changes first ---
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // --- Step 4: Recalculate TotalVotes and LikesCount from actual DB counts (self-healing) ---
            poll.TotalVotes = await _voteRepository.CountAsync(v => v.PollId == request.PollId, cancellationToken);
            await _validationPollRepository.UpdateAsync(poll);

            feedPost.LikesCount = await _unitOfWork.PostReactions.CountAsync(r => r.PostId == feedPost.Id, cancellationToken);
            await _unitOfWork.FeedPosts.UpdateAsync(feedPost);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try { await _socialHub.NotifyReactionUpdateAsync(feedPost.Id.ToString(), feedPost.LikesCount); } catch { }

            response.Id = voteId;
            response.Success = true;
            response.Message = "Vote submitted successfully";

            if (poll.UserId != request.UserId)
            {
                await _mediator.Send(new CreateNotificationCommand
                {
                    UserId = poll.UserId,
                    Request = new CreateNotificationDto
                    {
                        Type = NotificationType.Social,
                        Title = "New vote on your poll",
                        Message = "Someone voted on your poll.",
                        ActionUrl = $"/social/polls/{poll.Id}"
                    }
                });
            }

            _logger.LogInformation("Vote {Id} submitted by user {UserId} on poll {PollId}", voteId, request.UserId, request.PollId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting vote for user {UserId} on poll {PollId}", request.UserId, request.PollId);
            response.Success = false; response.Message = "Error submitting vote"; response.Errors.Add(ex.Message);
        }
        return response;
    }
}

public class UnVoteOnPollCommandhandler : IRequestHandler<UnVoteOnPollCommand, BaseCommandResponse>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IPollOptionRepository _pollOptionRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UnVoteOnPollCommandhandler> _logger;
    private readonly ISocialHubService _socialHub;

    public UnVoteOnPollCommandhandler(
        IValidationPollRepository validationPollRepository,
        IPollOptionRepository pollOptionRepository,
        IVoteRepository voteRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UnVoteOnPollCommandhandler> logger,
        ISocialHubService socialHub)
    {
        _validationPollRepository = validationPollRepository;
        _pollOptionRepository = pollOptionRepository;
        _voteRepository = voteRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _socialHub = socialHub;
    }

    public async Task<BaseCommandResponse> Handle(UnVoteOnPollCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        try
        {
            var option = await _pollOptionRepository.GetByIdAsync(request.Request.OptionId);
            if (option == null) { response.Success = false; response.Message = "Option not found"; response.Errors.Add("Option not found"); return response; }

            var vote = await _voteRepository.GetUserVoteByOptionId(request.UserId, request.Request.OptionId);
            if (vote == null) { response.Success = false; response.Message = "Vote not found"; response.Errors.Add("Vote not found"); return response; }

            var pollId = option.PollId;
            var feedPost = await _unitOfWork.FeedPosts.GetByPollIdAsync(pollId);

            // Soft-delete the vote
            await _voteRepository.DeleteVoteAsync(request.UserId, request.Request.OptionId);

            // Remove the reaction
            if (feedPost != null)
            {
                var reaction = await _unitOfWork.PostReactions.GetUserReaction(request.UserId, feedPost.Id);
                if (reaction != null) await _unitOfWork.PostReactions.RemoveAsync(reaction);
            }

            // Save vote + reaction removal first
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Recalculate TotalVotes and LikesCount from actual DB counts (self-healing)
            var poll = await _validationPollRepository.GetByIdAsync(pollId);
            if (poll != null)
            {
                poll.TotalVotes = await _voteRepository.CountAsync(v => v.PollId == pollId, cancellationToken);
                await _validationPollRepository.UpdateAsync(poll);
            }

            if (feedPost != null)
            {
                feedPost.LikesCount = await _unitOfWork.PostReactions.CountAsync(r => r.PostId == feedPost.Id, cancellationToken);
                await _unitOfWork.FeedPosts.UpdateAsync(feedPost);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try { await _socialHub.NotifyReactionUpdateAsync(feedPost?.Id.ToString() ?? pollId.ToString(), feedPost?.LikesCount ?? 0); } catch { }

            response.Success = true;
            response.Message = "Vote uncast successfully";
        }
        catch (Exception ex)
        {
            response.Success = false; response.Message = "Error uncasting vote"; response.Errors.Add(ex.Message);
        }
        return response;
    }
}
