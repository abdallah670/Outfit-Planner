using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class NotificationSettingsRepository : GenericRepository<NotificationSettings>, INotificationSettingsRepository
{
    public NotificationSettingsRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<NotificationSettings?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
