using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class ChatSessionRepository : GenericRepository<ChatSession>, IChatSessionRepository
{
    public ChatSessionRepository(AppDbContext context) : base(context)
    {
    }
    public async Task<ChatSession?> GetByIdWithMessagesAsync(Guid id)
    {
        return await _dbSet
            .Include(cs => cs.Messages)
            .FirstOrDefaultAsync(cs => cs.Id == id);
    }

    public Task<List<ChatSession>> GetByUserIdAsync(string userId)
    {
        return _dbSet
            .Where(cs => cs.UserId == userId)
            .Include(cs => cs.Messages)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, int skip, int take)
    {
        var messages = await _context.Set<ChatMessage>()
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
            
        return messages.OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        await _context.Set<ChatMessage>().AddAsync(message);
        await _context.SaveChangesAsync();
    }
}
