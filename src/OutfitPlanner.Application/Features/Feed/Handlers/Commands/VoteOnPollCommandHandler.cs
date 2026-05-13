using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Feed.Handlers.Commands;

/// <summary>
/// Handler for VoteOnPollCommand
/// </summary>
public class VoteOnPollCommandHandler : IRequestHandler<VoteOnPollCommand, BaseCommandResponse>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IPollOptionRepository _pollOptionRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUnitOfWork _unitOfWork ;
    private readonly IMapper _mapper;
    private readonly ILogger<VoteOnPollCommandHandler> _logger;

    public VoteOnPollCommandHandler(
        IValidationPollRepository validationPollRepository,
        IPollOptionRepository pollOptionRepository,
        IVoteRepository voteRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<VoteOnPollCommandHandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _pollOptionRepository = pollOptionRepository;
        _voteRepository = voteRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(VoteOnPollCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            // 1. Verify poll exists and is active and not expired
            var poll = await _validationPollRepository.GetByIdAsync(request.PollId);
            if (poll == null)
            {
                response.Success = false;
                response.Message = "Poll not found";
                response.Errors.Add("Poll not found");
                return response;
            }

            if (poll.Status != PollStatus.Active)
            {
                response.Success = false;
                response.Message = "Poll is not active";
                response.Errors.Add("Poll is not active");
                return response;
            }

            if (poll.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                response.Success = false;
                response.Message = "Poll has expired";
                response.Errors.Add("Poll has expired");
                return response;
            }

            // 2. Verify option belongs to this poll
            var option = await _pollOptionRepository.GetByIdAsync(request.Request.OptionId);
            if (option == null || option.PollId != request.PollId)
            {
                response.Success = false;
                response.Message = "Option not found in this poll";
                response.Errors.Add("Option not found in this poll");
                return response;
            }

            // 3. Check user  already voted other options to toggle to current option
            var hasVoted = await _voteRepository.GetUserVote(request.UserId, request.PollId);
            if (hasVoted != null)
            {
                if(hasVoted.OptionId != request.Request.OptionId)
                {
                    // Remove the prev vote
                    await _voteRepository.RemoveAsync(hasVoted);
                  
                }
            }

            // 4. Create vote entity
            var vote = new Vote
            {
                PollId = request.PollId,
                OptionId = request.Request.OptionId,
                VoterId = request.UserId,
               
               
            };
            // 5. Create a reaction for feed post
            var feedPost = await _unitOfWork.FeedPosts.GetByPollIdAsync(request.PollId);
            if (feedPost == null)
            {
                response.Success = false;
                response.Message = "Feed post not found";
                response.Errors.Add("Feed post not found");
                return response;
            }
            var reaction = new PostReaction
            {
                PostId = feedPost.Id,
                UserId = request.UserId,
                ReactionType = ReactionType.Heart
            };
            // 6. Save vote
            await _voteRepository.AddAsync(vote);
            await _unitOfWork.PostReactions.AddAsync(reaction);
            
            poll.TotalVotes++;
            await _validationPollRepository.UpdateAsync(poll);
            
            feedPost.LikesCount++;
            await _unitOfWork.FeedPosts.UpdateAsync(feedPost);

            await _unitOfWork.SaveChangesAsync();
            response.Id = vote.Id;
            response.Success = true;
            response.Message = "Vote submitted successfully";
            
            _logger.LogInformation("Vote {Id} submitted by user {UserId} on poll {PollId}", 
                vote.Id, request.UserId, request.PollId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting vote for user {UserId} on poll {PollId}", 
                request.UserId, request.PollId);
            response.Success = false;
            response.Message = "Error submitting vote";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
//Uncast vote for the same option

public class UnVoteOnPollCommandhandler : IRequestHandler<UnVoteOnPollCommand, BaseCommandResponse>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IPollOptionRepository _pollOptionRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UnVoteOnPollCommandhandler> _logger;

    public UnVoteOnPollCommandhandler(
        IValidationPollRepository validationPollRepository,
        IPollOptionRepository pollOptionRepository,
        IVoteRepository voteRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UnVoteOnPollCommandhandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _pollOptionRepository = pollOptionRepository;
        _voteRepository = voteRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(UnVoteOnPollCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
    {
        //check option existed and user voted 
        var option = await _pollOptionRepository.GetByIdAsync(request.Request.OptionId);
        if (option == null)
        {
            response.Success = false;
            response.Message = "Option not found";
            response.Errors.Add("Option not found");
            return response;
        }
        var vote = await _voteRepository.GetUserVoteByOptionId(request.UserId, request.Request.OptionId);
        if (vote == null)
        {
            response.Success = false;
            response.Message = "Vote not found";
            response.Errors.Add("Vote not found");
            return response;
        }
        //remove user vote by option id
        await _voteRepository.DeleteVoteAsync(request.UserId, request.Request.OptionId);
        var pollId = option.PollId;
        var poll = await _validationPollRepository.GetByIdAsync(pollId);
        if (poll != null)
        {
            poll.TotalVotes--;
            await _validationPollRepository.UpdateAsync(poll);
        }
        //remove user reaction for feed post
        var feedPost = await _unitOfWork.FeedPosts.GetByPollIdAsync(pollId);
        if (feedPost != null)
        {
            var reaction = await _unitOfWork.PostReactions.GetUserReaction(request.UserId, feedPost.Id);
            if (reaction != null)
            {
                await _unitOfWork.PostReactions.RemoveAsync(reaction);
            }
            feedPost.LikesCount--;
            await _unitOfWork.FeedPosts.UpdateAsync(feedPost);
        }
        
        response.Success = true;
        response.Message = "Vote uncast successfully";
    }
    catch (Exception ex)
    {
        response.Success = false;
        response.Message = "Error uncasting vote";
        response.Errors.Add(ex.Message);
    }

    return response;
     }
};
