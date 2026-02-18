using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IOutfitRepository : IGenericRepository<Outfit>
{
    Task<IEnumerable<Outfit>> GetByUserIdAsync(string userId);
    Task<Outfit?> GetWithItemsByIdAsync(Guid id);
}
