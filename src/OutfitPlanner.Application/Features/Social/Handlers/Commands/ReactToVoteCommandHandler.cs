using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Social.Handlers.Commands;

public class ReactToVoteCommandHandler : IRequestHandler<ReactToVoteCommand, BaseCommandResponse>
{
    private readonly IVoteRepository _voteRepository;
    private readonly IVoteReactionRepository _voteReactionRepository;
    private readonly ILogger<ReactToVoteCommandHandler> _logger;

    public ReactToVoteCommandHandler(
        IVoteRepository voteRepository,
        IVoteReactionRepository voteReactionRepository,
        ILogger<ReactToVoteCommandHandler> logger)
    {
        _voteRepository = voteRepository;
        _voteReactionRepository = voteReactionRepository;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(ReactToVoteCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var vote = await _voteRepository.GetByIdAsync(request.VoteId);
            if (vote == null)
            {
                response.Success = false;
                response.Message = "Vote not found";
                response.Errors.Add("Vote not found");
                return response;
            }

            var existingReaction = await _voteReactionRepository.GetUserReactionForVoteAsync(request.VoteId, request.UserId);

            if (!Enum.TryParse<ReactionType>(request.Request.ReactionType, true, out var parsedReactionType))
            {
                response.Success = false;
                response.Message = "Invalid reaction type";
                response.Errors.Add("Invalid reaction type provided");
                return response;
            }

            if (existingReaction != null)
            {
                if (existingReaction.ReactionType == parsedReactionType)
                {
                    response.Success = true;
                    response.Message = "Reaction unchanged";
                    response.Id = existingReaction.Id;
                    return response;
                }
                
                existingReaction.ReactionType = parsedReactionType;
                existingReaction.CreatedAt = DateTimeOffset.UtcNow;
                await _voteReactionRepository.UpdateAsync(existingReaction);
                
                response.Success = true;
                response.Id = existingReaction.Id;
                response.Message = "Reaction updated";
            }
            else
            {
                var newReaction = new VoteReaction
                {
                    VoteId = request.VoteId,
                    UserId = request.UserId,
                    ReactionType = parsedReactionType
                };
                await _voteReactionRepository.AddAsync(newReaction);
                
                response.Success = true;
                response.Id = newReaction.Id;
                response.Message = "Reaction added";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reaction for user {UserId} on vote {VoteId}", request.UserId, request.VoteId);
            response.Success = false;
            response.Message = "Error processing reaction";
            response.Errors.Add(ex.Message);
        }

        return response;
    }
}
