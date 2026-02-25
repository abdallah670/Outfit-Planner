using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IClothingItemRepository : IGenericRepository<ClothingItem>
{
    Task<IEnumerable<ClothingItem>> GetByUserIdAsync(string userId);
    Task<IEnumerable<ClothingItem>> GetByCategoryAsync(string userId, string category);
    
}
