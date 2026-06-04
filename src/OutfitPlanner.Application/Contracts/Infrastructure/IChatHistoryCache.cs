namespace OutfitPlanner.Application.Contracts.Infrastructure;

public class CachedChatSession
{
    public Guid SessionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CachedChatMessage> Messages { get; set; } = new();
    public DateTimeOffset LastActivity { get; set; }
}

public class CachedChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public interface IChatHistoryCache
{
    Task<CachedChatSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task SetSessionAsync(CachedChatSession session, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Guid sessionId, CachedChatMessage message, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<List<CachedChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count = 5, CancellationToken cancellationToken = default);
}