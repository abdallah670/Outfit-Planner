using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IAppPreferencesRepository : IGenericRepository<AppPreferences>
{
    Task<AppPreferences?> GetByUserIdAsync(string userId);
}
