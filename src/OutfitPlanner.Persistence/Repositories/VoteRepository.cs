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

    public async Task<Vote?> GetUserVote(string userId, Guid pollId)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.VoterId == userId && v.PollId == pollId);
    }
    public async Task<Vote?> GetUserVoteByOptionId(string userId, Guid optionId)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.VoterId == userId && v.OptionId == optionId);
    }
    //delete vote by option id and voter id
    public async Task DeleteVoteAsync(string voterId, Guid optionId)
    {
        var vote = await _dbSet.FirstOrDefaultAsync(v => v.VoterId == voterId && v.OptionId == optionId);
        if (vote != null)
        {
            await RemoveAsync(vote);
        }
    }

    public async Task<IEnumerable<(Vote Vote, string VoterName, string? VoterAvatarUrl)>> GetVotersWithDetailsAsync(Guid pollId, Guid? optionId = null)
    {
        var query = _dbSet
            .Where(v => v.PollId == pollId)
            .Include(v => v.Voter)
            .Include(v => v.Option)
                .ThenInclude(o => o.Outfit)
            .AsQueryable();

        if (optionId.HasValue)
        {
            query = query.Where(v => v.OptionId == optionId.Value);
        }

        var votes = await query.ToListAsync();

        return votes.Select(v => (
            Vote: v,
            VoterName: v.Voter?.Name ?? "Unknown",
            VoterAvatarUrl: v.Voter?.ProfilePictureUrl
        ));
    }
}
