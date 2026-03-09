using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class OutfitItemRepository : GenericRepository<OutfitItem>, IOutfitItemRepository
{
    public OutfitItemRepository(AppDbContext context) : base(context)
    {
    }

    // Ensure the entity is attached to the same DbContext before removing it so EF can emit a valid DELETE.
    // Do not call SaveChanges here; the UnitOfWork owns committing.
    public Task Delete(OutfitItem item)
    {
       
        _dbSet.Remove(item);
        return _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OutfitItem>> GetByOutfitIdAsync(Guid outfitId)
    {
        return await _dbSet.Where(i => i.OutfitId == outfitId).ToListAsync();
    }

    /// <summary>
    /// Deletes all OutfitItems for the given outfit directly via SQL.
    /// Bypasses the EF change tracker entirely to avoid concurrency issues.
    /// </summary>
    public async Task<int> DeleteByOutfitIdAsync(Guid outfitId)
    {
        return await _dbSet
            .Where(i => i.OutfitId == outfitId)
            .ExecuteDeleteAsync();
    }
}
