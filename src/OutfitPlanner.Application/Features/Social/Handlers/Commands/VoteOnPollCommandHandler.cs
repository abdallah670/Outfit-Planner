using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

/// <summary>
/// Handler for VoteOnPollCommand
/// </summary>
public class VoteOnPollCommandHandler : IRequestHandler<VoteOnPollCommand, BaseCommandResponse>
{
    private readonly IValidationPollRepository _validationPollRepository;
    private readonly IPollOptionRepository _pollOptionRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VoteOnPollCommandHandler> _logger;

    public VoteOnPollCommandHandler(
        IValidationPollRepository validationPollRepository,
        IPollOptionRepository pollOptionRepository,
        IVoteRepository voteRepository,
        IMapper mapper,
        ILogger<VoteOnPollCommandHandler> logger)
    {
        _validationPollRepository = validationPollRepository;
        _pollOptionRepository = pollOptionRepository;
        _voteRepository = voteRepository;
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

            // 3. Check user hasn't already voted on this poll (efficient server-side check)
            var hasVoted = await _voteRepository.HasUserVotedAsync(request.PollId, request.UserId);
            if (hasVoted)
            {
                response.Success = false;
                response.Message = "You have already voted on this poll";
                response.Errors.Add("You have already voted on this poll");
                return response;
            }

            // 4. Create vote entity
            var vote = new Vote
            {
                PollId = request.PollId,
                OptionId = request.Request.OptionId,
                VoterId = request.UserId,
                Rating = request.Request.Rating,
                Comment = request.Request.Comment,
                IsAnonymous = request.Request.IsAnonymous
            };

            // 5. Save vote
            await _voteRepository.AddAsync(vote);

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
