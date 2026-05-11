using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IUserActivityRepository : IGenericRepository<UserActivity>
{
    Task<IEnumerable<UserActivity>> GetByUserIdAsync(string userId, int limit = 100);
    Task<IEnumerable<UserActivity>> GetByTypeAsync(ActivityType type, int limit = 100);
    Task<IEnumerable<UserActivity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCountByTypeAsync(ActivityType type);
    Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 50);
}
