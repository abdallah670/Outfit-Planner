using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;
using System;
using System.Threading.Tasks;

namespace OutfitPlanner.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;

    public CacheService(IMemoryCache cache, IOptions<CacheSettings> settings)
    {
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
    {
        if (_cache.TryGetValue(key, out T? cached))
        {
            return cached;
        }

        var result = await factory();
        
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes)
        };
        
        if (_settings.EnableSlidingExpiration && !ttl.HasValue)
        {
            options.SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes);
        }

        _cache.Set(key, result, options);
        return result;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
