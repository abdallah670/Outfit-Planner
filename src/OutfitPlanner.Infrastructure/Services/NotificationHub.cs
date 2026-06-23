using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Infrastructure.Services;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            _logger.LogInformation("NotificationHub: User {UserId} connected (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            _logger.LogInformation("NotificationHub: User {UserId} disconnected (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Server calls this to push a notification to a specific user.
    /// Frontend listens for "ReceiveNotification".
    /// </summary>
    public async Task SendNotification(string userId, object notification)
    {
        await Clients.Group(userId).SendAsync("ReceiveNotification", notification);
    }
}