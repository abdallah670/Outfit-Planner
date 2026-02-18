using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class StyleRuleRepository : GenericRepository<StyleRule>, IStyleRuleRepository
{
    public StyleRuleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StyleRule>> GetByProfileIdAsync(Guid profileId)
    {
        return await _dbSet.Where(r => r.UserStyleProfileId == profileId).ToListAsync();
    }
}
