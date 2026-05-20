using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface ITrendingOutfitRepository : IGenericRepository<TrendingOutfit>
{
    Task<IEnumerable<TrendingOutfit>> GetTrendingByLocationAsync(string location, int count = 10);
    Task<IEnumerable<TrendingOutfit>> GetGlobalTrendingAsync(int count = 20);
    Task<(IEnumerable<TrendingOutfit> Items, int TotalCount)> GetGlobalTrendingPagedAsync(int page, int pageSize);
    Task<CursorPagination.CursorPagedResult<TrendingOutfit>> GetGlobalTrendingCursorAsync(string? cursor, int pageSize);

    Task<IEnumerable<TrendingOutfit>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
