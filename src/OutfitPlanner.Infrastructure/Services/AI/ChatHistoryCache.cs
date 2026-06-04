using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class ChatHistoryCache : IChatHistoryCache
{
    private readonly IMemoryCache _cache;
    private readonly AISettings _settings;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public ChatHistoryCache(IMemoryCache cache, IOptions<AISettings> settings)
    {
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<CachedChatSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var key = GetSessionKey(sessionId);
        if (_cache.TryGetValue(key, out CachedChatSession? session))
            return session;

        return null;
    }

    public async Task SetSessionAsync(CachedChatSession session, CancellationToken cancellationToken = default)
    {
        var key = GetSessionKey(session.SessionId);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CacheMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        _cache.Set(key, session, cacheOptions);
    }

    public async Task AddMessageAsync(Guid sessionId, CachedChatMessage message, CancellationToken cancellationToken = default)
    {
        var semaphore = _locks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var session = await GetSessionAsync(sessionId, cancellationToken);
            if (session == null)
            {
                session = new CachedChatSession
                {
                    SessionId = sessionId,
                    UserId = message.Role == "user" ? "" : "",
                    Messages = new List<CachedChatMessage>(),
                    LastActivity = DateTimeOffset.UtcNow
                };
            }

            session.Messages.Add(message);
            session.LastActivity = DateTimeOffset.UtcNow;

            // Trim to max history
            if (session.Messages.Count > _settings.MaxHistoryMessages)
            {
                session.Messages = session.Messages
                    .Skip(session.Messages.Count - _settings.MaxHistoryMessages)
                    .ToList();
            }

            await SetSessionAsync(session, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var key = GetSessionKey(sessionId);
        _cache.Remove(key);
        _locks.TryRemove(sessionId, out _);
    }

    public async Task<List<CachedChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count = 5, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync(sessionId, cancellationToken);
        if (session?.Messages == null || !session.Messages.Any())
            return new List<CachedChatMessage>();

        return session.Messages
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .OrderBy(m => m.Timestamp)
            .ToList();
    }

    private static string GetSessionKey(Guid sessionId) => $"chat_session_{sessionId}";
}