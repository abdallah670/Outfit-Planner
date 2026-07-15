using System.Collections.Generic;
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

    public async Task SendNotificationToUsersAsync(IEnumerable<string> userIds, object notification)
    {
        var ids = userIds?.ToList() ?? new List<string>();
        if (ids.Count == 0) return;

        // One SignalR call reaches every mentioned user's group at once.
        await _hubContext.Clients.Groups(ids).SendAsync("ReceiveNotification", notification);
    }
}
