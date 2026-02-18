using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class ClothingTagRepository : GenericRepository<ClothingTag>, IClothingTagRepository
{
    public ClothingTagRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClothingTag>> GetByClothingItemIdAsync(Guid clothingItemId)
    {
        return await _dbSet.Where(t => t.ClothingItemId == clothingItemId).ToListAsync();
    }
}
