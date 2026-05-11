namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface ICacheManagementService
{
    /// <summary>
    /// Clears all cache entries
    /// </summary>
    Task ClearAllCacheAsync();

    /// <summary>
    /// Clears cache entries by specific key
    /// </summary>
    Task ClearCacheByKeyAsync(string key);

    /// <summary>
    /// Clears cache entries by key pattern (supports wildcards)
    /// </summary>
    Task ClearCacheByPatternAsync(string pattern);

    /// <summary>
    /// Gets cache statistics including total entries and memory usage
    /// </summary>
    Task<CacheStatistics> GetCacheStatisticsAsync();

    /// <summary>
    /// Gets a list of all cache keys
    /// </summary>
    Task<IEnumerable<string>> GetAllCacheKeysAsync();

    /// <summary>
    /// Checks if a specific cache key exists
    /// </summary>
    Task<bool> CacheKeyExistsAsync(string key);

    /// <summary>
    /// Gets the size of a specific cache entry
    /// </summary>
    Task<long> GetCacheEntrySizeAsync(string key);
}

public record CacheStatistics(
    int TotalEntries,
    long TotalMemoryUsage,
    int HitCount,
    int MissCount,
    double HitRatio,
    DateTime LastCleanup
);
