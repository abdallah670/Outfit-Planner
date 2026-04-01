using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class VoteReactionRepository : GenericRepository<VoteReaction>, IVoteReactionRepository
{
    private readonly AppDbContext _context;

    public VoteReactionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<VoteReaction?> GetUserReactionForVoteAsync(Guid voteId, string userId)
    {
        return await _context.VoteReactions
            .FirstOrDefaultAsync(r => r.VoteId == voteId && r.UserId == userId);
    }

    public async Task<IEnumerable<VoteReaction>> GetReactionsForVoteAsync(Guid voteId)
    {
        return await _context.VoteReactions
            .Where(r => r.VoteId == voteId)
            .ToListAsync();
    }
}
