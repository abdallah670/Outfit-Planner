using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class OutfitFeedbackRepository : GenericRepository<OutfitFeedback>, IOutfitFeedbackRepository
{
    public OutfitFeedbackRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OutfitFeedback>> GetByOutfitIdAsync(Guid outfitId)
    {
        return await _dbSet
            .Where(f => f.OutfitId == outfitId)
            .Include(f => f.Reviewer)
            .ToListAsync();
    }
}
