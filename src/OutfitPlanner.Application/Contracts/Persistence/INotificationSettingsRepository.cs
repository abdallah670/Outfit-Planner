using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface INotificationSettingsRepository : IGenericRepository<NotificationSettings>
{
    Task<NotificationSettings?> GetByUserIdAsync(string userId);
}
