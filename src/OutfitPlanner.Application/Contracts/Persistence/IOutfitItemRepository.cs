using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IOutfitItemRepository : IGenericRepository<OutfitItem>
{
    Task<IEnumerable<OutfitItem>> GetByOutfitIdAsync(Guid outfitId);
    Task Delete(OutfitItem item);
    Task<int> DeleteByOutfitIdAsync(Guid outfitId);
}
