using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IClothingTagRepository : IGenericRepository<ClothingTag>
{
    Task<IEnumerable<ClothingTag>> GetByClothingItemIdAsync(Guid clothingItemId);
}
