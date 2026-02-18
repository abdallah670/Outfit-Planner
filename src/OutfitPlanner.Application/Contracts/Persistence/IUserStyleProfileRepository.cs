using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IUserStyleProfileRepository : IGenericRepository<UserStyleProfile>
{
    Task<UserStyleProfile?> GetByUserIdAsync(string userId);
}
