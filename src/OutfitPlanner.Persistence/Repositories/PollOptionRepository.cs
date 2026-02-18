using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class PollOptionRepository : GenericRepository<PollOption>, IPollOptionRepository
{
    public PollOptionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PollOption>> GetByPollIdAsync(Guid pollId)
    {
        return await _dbSet.Where(o => o.PollId == pollId).ToListAsync();
    }
}
