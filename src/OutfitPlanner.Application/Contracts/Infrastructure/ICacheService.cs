using System;
using System.Threading.Tasks;

namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null);
    void Remove(string key);
}
