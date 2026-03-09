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
            .Where(i => i.UserId == userId && i.IsActive)
            .Include(i => i.Tags)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClothingItem>> GetByCategoryAsync(string userId, string category)
    {
        return await _dbSet
            .Where(i => i.UserId == userId && i.Category == category && i.IsActive)
            .Include(i => i.Tags)
            .ToListAsync();
    }
    // Delete is a hard delete,deletion is permanent and removes the record from the database. The method is asynchronous and returns a Task, indicating that it can be awaited.
    public async Task Delete(ClothingItem item)
    {
        _dbSet.Remove(item);
        await _context.SaveChangesAsync();
    }
}
