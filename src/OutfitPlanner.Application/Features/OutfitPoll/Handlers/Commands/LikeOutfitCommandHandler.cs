using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.OutfitPoll.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Application.Features.OutfitPoll.Handlers.Commands;

public class LikeOutfitCommandHandler : IRequestHandler<LikeOutfitCommand, OutfitVoteResultDto>
{
    private readonly AppDbContext _context;

    public LikeOutfitCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OutfitVoteResultDto> Handle(LikeOutfitCommand request, CancellationToken cancellationToken)
    {
        // Find or create outfit poll
        var poll = await GetOrCreateOutfitPollAsync(request.OutfitId);
        if (poll == null)
            return new OutfitVoteResultDto { ErrorMessage = "Outfit not found" };

        var option = poll.Options.FirstOrDefault();
        if (option == null)
            return new OutfitVoteResultDto { ErrorMessage = "Invalid outfit poll configuration" };

        // Check if user already voted
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.OptionId == option.Id && v.VoterId == request.UserId, cancellationToken);

        if (existingVote != null)
        {
            // Already liked, return current state
            return new OutfitVoteResultDto
            {
                Success = true,
                Message = "Already liked",
                UserHasVoted = true,
                VoteCount = await GetVoteCountAsync(option.Id),
                RatingGiven = existingVote.Rating ?? 0
            };
        }

        // Create new vote
        var vote = new Vote
        {
            Id = Guid.NewGuid(),
            OptionId = option.Id,
            VoterId = request.UserId,
            Rating = request.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync(cancellationToken);

        return new OutfitVoteResultDto
        {
            Success = true,
            Message = "Liked successfully",
            UserHasVoted = true,
            VoteCount = await GetVoteCountAsync(option.Id),
            RatingGiven = request.Rating ?? 0
        };
    }

    private async Task<int> GetVoteCountAsync(Guid optionId)
    {
        return await _context.Votes.CountAsync(v => v.OptionId == optionId);
    }

    private async Task<ValidationPoll?> GetOrCreateOutfitPollAsync(Guid outfitId)
    {
        var poll = await _context.ValidationPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Context.Contains($"\"outfitId\":\"{outfitId}\"") || 
                                      p.Options.Any(o => o.OutfitId == outfitId));

        if (poll != null) return poll;

        // Create new poll for the outfit
        poll = new ValidationPoll
        {
            Id = Guid.NewGuid(),
            Question = "Outfit Rating",
            Context = $"{{\"outfitId\":\"{outfitId}\"}}",
            Status = PollStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            Options = new List<PollOption>
            {
                new PollOption
                {
                    Id = Guid.NewGuid(),
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _context.ValidationPolls.Add(poll);
        await _context.SaveChangesAsync();
        return poll;
    }
}
