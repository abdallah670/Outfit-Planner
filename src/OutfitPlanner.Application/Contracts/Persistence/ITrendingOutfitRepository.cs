using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface ITrendingOutfitRepository : IGenericRepository<TrendingOutfit>
{
    Task<IEnumerable<TrendingOutfit>> GetTrendingByLocationAsync(string location, int count = 10);
    Task<IEnumerable<TrendingOutfit>> GetGlobalTrendingAsync(int count = 20);
    Task<IEnumerable<TrendingOutfit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
