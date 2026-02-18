using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IUserPreferencesRepository : IGenericRepository<UserPreferences>
{
    Task<UserPreferences?> GetByUserIdAsync(string userId);
}
