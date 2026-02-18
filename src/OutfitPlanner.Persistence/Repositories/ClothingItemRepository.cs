using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Persistence.Repositories;

public class ClothingItemRepository : GenericRepository<ClothingItem>, IClothingItemRepository
{
    public ClothingItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClothingItem>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(i => i.UserId == userId)
            .Include(i => i.Tags)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClothingItem>> GetByCategoryAsync(string userId, string category)
    {
        return await _dbSet
            .Where(i => i.UserId == userId && i.Category == category)
            .Include(i => i.Tags)
            .ToListAsync();
    }
}
