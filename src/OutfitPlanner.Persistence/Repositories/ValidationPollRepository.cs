using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Persistence.Repositories;

public class ValidationPollRepository : GenericRepository<ValidationPoll>, IValidationPollRepository
{
    public ValidationPollRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ValidationPoll>> GetActivePollsAsync()
    {
        return await _dbSet
            .Where(p => p.Status == PollStatus.Active && p.ExpiresAt > DateTimeOffset.UtcNow)
            .Include(p => p.Options)
            .ToListAsync();
    }

    public async Task<IEnumerable<ValidationPoll>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .Include(p => p.Options)
            .ToListAsync();
    }

    public async Task<IEnumerable<ValidationPoll>> GetPollsForTrendingAsync()
    {
        return await _dbSet
            .Include(p => p.Options)
                .ThenInclude(o => o.Votes)
            .ToListAsync();
    }

    public async Task<ValidationPoll?> GetMostVotedActivePollAsync()
    {
        return await _dbSet
            .Where(p => p.Status == PollStatus.Active && p.ExpiresAt > DateTimeOffset.UtcNow)
            .Include(p => p.Options)
                .ThenInclude(o => o.Outfit)
            .Include(p => p.User)
            .Include(p => p.Votes)
            .OrderByDescending(p => p.Votes.Count)
            .ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
