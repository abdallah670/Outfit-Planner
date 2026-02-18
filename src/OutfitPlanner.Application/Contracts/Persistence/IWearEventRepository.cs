using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IWearEventRepository : IGenericRepository<WearEvent>
{
    Task<IEnumerable<WearEvent>> GetByUserIdAsync(string userId);
    Task<IEnumerable<WearEvent>> GetByClothingItemIdAsync(Guid clothingItemId);
}
