using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class VoteRepository : GenericRepository<Vote>, IVoteRepository
{
    public VoteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Vote>> GetByPollIdAsync(Guid pollId)
    {
        return await _dbSet.Where(v => v.PollId == pollId).ToListAsync();
    }

    public async Task<IEnumerable<Vote>> GetByOptionIdAsync(Guid optionId)
    {
        return await _dbSet.Where(v => v.OptionId == optionId).ToListAsync();
    }

    public async Task<bool> HasUserVotedAsync(Guid pollId, string userId)
    {
        return await _dbSet.AnyAsync(v => v.PollId == pollId && v.VoterId == userId);
    }
}
