using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.OutfitPoll.Requests.Commands;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.OutfitPoll.Handlers.Commands;

public class LikeOutfitCommandHandler : IRequestHandler<LikeOutfitCommand, OutfitVoteResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public LikeOutfitCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OutfitVoteResultDto> Handle(LikeOutfitCommand request, CancellationToken cancellationToken)
    {
        // Find outfit poll
        var poll = await _unitOfWork.ValidationPolls
            .FirstOrDefaultAsync(p => p.Options.Any(o => o.OutfitId == request.OutfitId));

        if (poll == null)
        {
            return new OutfitVoteResultDto
            {
                OutfitId = request.OutfitId,
                VoteCount = 0,
                UserHasVoted = false
            };
        }

        var option = poll.Options.FirstOrDefault(o => o.OutfitId == request.OutfitId);
        if (option == null)
        {
            return new OutfitVoteResultDto
            {
                OutfitId = request.OutfitId,
                VoteCount = 0,
                UserHasVoted = false
            };
        }

        // Check if user already voted
        var existingVote = await _unitOfWork.Votes.FirstOrDefaultAsync(
            v => v.OptionId == option.Id && v.VoterId == request.UserId);

        if (existingVote == null)
        {
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                OptionId = option.Id,
                VoterId = request.UserId,
                Rating = request.Rating ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Votes.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Get all votes for this option
        var allVotes = await _unitOfWork.Votes.GetByOptionIdAsync(option.Id);
        int voteCount = allVotes.Count();

        return new OutfitVoteResultDto
        {
            OutfitId = request.OutfitId,
            VoteCount = voteCount,
            UserHasVoted = true
        };
    }
}
