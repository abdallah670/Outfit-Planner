using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
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
            .Where(p => p.CreatedBy == userId)
            .Include(p => p.Options)
            .ToListAsync();
    }

    public async Task<IEnumerable<ValidationPoll>> GetPollsForTrendingAsync()
    {
        return await _dbSet
            .Include(p => p.Options)
                .ThenInclude(o => o.Votes)
                    .ThenInclude(v => v.Reactions)
            .ToListAsync();
    }
}
