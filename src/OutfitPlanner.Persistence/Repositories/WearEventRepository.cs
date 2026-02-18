using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class WearEventRepository : GenericRepository<WearEvent>, IWearEventRepository
{
    public WearEventRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WearEvent>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WearEvent>> GetByClothingItemIdAsync(Guid clothingItemId)
    {
        return await _dbSet
            .Where(e => e.ClothingItemId == clothingItemId)
            .ToListAsync();
    }
}
