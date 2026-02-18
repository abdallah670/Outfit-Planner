using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class OutfitItemRepository : GenericRepository<OutfitItem>, IOutfitItemRepository
{
    public OutfitItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OutfitItem>> GetByOutfitIdAsync(Guid outfitId)
    {
        return await _dbSet.Where(i => i.OutfitId == outfitId).ToListAsync();
    }
}
