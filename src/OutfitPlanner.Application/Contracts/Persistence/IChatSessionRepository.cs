using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IChatSessionRepository : IGenericRepository<ChatSession>
{
    Task<ChatSession?> GetByIdWithMessagesAsync(Guid id);
    Task<List<ChatSession>> GetByUserIdAsync(string userId);
    Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, int skip, int take);
    Task AddMessageAsync(ChatMessage message);
}
