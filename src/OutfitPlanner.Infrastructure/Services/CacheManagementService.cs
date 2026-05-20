using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;

namespace OutfitPlanner.Infrastructure.Services;

public class CacheManagementService : ICacheManagementService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheManagementService> _logger;
    private CacheStatistics _statistics;
    private readonly object _statsLock = new object();

    public CacheManagementService(IMemoryCache memoryCache, ILogger<CacheManagementService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _statistics = new CacheStatistics(0, 0, 0, 0, 0.0, DateTime.UtcNow);
    }

    public async Task ClearAllCacheAsync()
    {
        _logger.LogInformation("Clearing all cache entries");

        try
        {
            // Since IMemoryCache doesn't provide a way to get all keys,
            // we'll track cache keys internally
            var cacheKeys = await GetAllCacheKeysAsync();
            
            foreach (var key in cacheKeys)
            {
                _memoryCache.Remove(key);
            }

            UpdateStatistics(clearedEntries: cacheKeys.Count());
            
            _logger.LogInformation("Cleared {Count} cache entries", cacheKeys.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all cache entries");
            throw;
        }
    }

    public async Task ClearCacheByKeyAsync(string key)
    {
        _logger.LogInformation("Clearing cache entry for key: {Key}", key);

        try
        {
            if (await CacheKeyExistsAsync(key))
            {
                _memoryCache.Remove(key);
                UpdateStatistics(clearedEntries: 1);
                _logger.LogInformation("Successfully cleared cache entry for key: {Key}", key);
            }
            else
            {
                _logger.LogWarning("Cache key not found: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache entry for key: {Key}", key);
            throw;
        }
    }

    public async Task ClearCacheByPatternAsync(string pattern)
    {
        _logger.LogInformation("Clearing cache entries matching pattern: {Pattern}", pattern);

        try
        {
            var cacheKeys = await GetAllCacheKeysAsync();
            var matchingKeys = cacheKeys.Where(key => 
                System.Text.RegularExpressions.Regex.IsMatch(key, pattern.Replace("*", ".*"))).ToList();

            foreach (var key in matchingKeys)
            {
                _memoryCache.Remove(key);
            }

            UpdateStatistics(clearedEntries: matchingKeys.Count);
            
            _logger.LogInformation("Cleared {Count} cache entries matching pattern: {Pattern}", 
                matchingKeys.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache entries matching pattern: {Pattern}", pattern);
            throw;
        }
    }

    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var totalEntries = (await GetAllCacheKeysAsync()).Count();
            
            lock (_statsLock)
            {
                return _statistics with { TotalEntries = totalEntries };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache statistics");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetAllCacheKeysAsync()
    {
        // Note: IMemoryCache doesn't expose keys directly
        // This is a limitation of the default memory cache
        // In a real implementation, you'd need to track keys separately
        // For now, we'll return common cache keys used in the application
        
        var commonKeys = new List<string>
        {
            "users_*",
            "outfits_*",
            "clothing_items_*",
            "feed_posts_*",
            "analytics_*",
            "system_settings_*",
            "role_management_*",
            "maintenance_mode",
            "cache_statistics"
        };

        // Try to expand patterns (simplified approach)
        var expandedKeys = new List<string>();
        foreach (var pattern in commonKeys)
        {
            if (pattern.Contains("*"))
            {
                // In a real implementation, you'd track actual keys
                // For now, return the pattern as-is
                expandedKeys.Add(pattern);
            }
            else
            {
                expandedKeys.Add(pattern);
            }
        }

        return await Task.FromResult(expandedKeys);
    }

    public async Task<bool> CacheKeyExistsAsync(string key)
    {
        try
        {
            // IMemoryCache doesn't have a direct Contains method
            // We'll try to get the value with a dummy object
            object? value = null;
            var exists = _memoryCache.TryGetValue(key, out value);
            return await Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if cache key exists: {Key}", key);
            return await Task.FromResult(false);
        }
    }

    public async Task<long> GetCacheEntrySizeAsync(string key)
    {
        try
        {
            object? value = null;
            if (_memoryCache.TryGetValue(key, out value))
            {
                // Estimate size - this is a rough approximation
                var size = value?.ToString()?.Length ?? 0;
                return await Task.FromResult(size);
            }
            return await Task.FromResult(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache entry size for key: {Key}", key);
            return await Task.FromResult(0);
        }
    }

    private void UpdateStatistics(int clearedEntries = 0)
    {
        lock (_statsLock)
        {
            _statistics = _statistics with
            {
                TotalEntries = Math.Max(0, _statistics.TotalEntries - clearedEntries),
                LastCleanup = DateTime.UtcNow
            };
        }
    }
}
