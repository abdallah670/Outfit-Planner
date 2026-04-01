using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Persistence.Repositories;

public class OutfitRepository : GenericRepository<Outfit>, IOutfitRepository
{
    public OutfitRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Outfit>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(i => i.ClothingItem)
            .ToListAsync();
    }

    public async Task<Outfit?> GetWithItemsByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Items)
                .ThenInclude(i => i.ClothingItem)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
