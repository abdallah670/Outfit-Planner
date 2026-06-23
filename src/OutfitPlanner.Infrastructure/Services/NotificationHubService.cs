using Microsoft.AspNetCore.SignalR;
using OutfitPlanner.Application.Contracts.Infrastructure;

namespace OutfitPlanner.Infrastructure.Services;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, object notification)
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
    }
}