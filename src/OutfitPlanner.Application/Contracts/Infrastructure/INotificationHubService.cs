namespace OutfitPlanner.Application.Contracts.Infrastructure;

/// <summary>
/// Abstraction for pushing real-time notifications via SignalR.
/// Implemented in Infrastructure layer to avoid SignalR dependency in Application.
/// </summary>
public interface INotificationHubService
{
    Task SendNotificationAsync(string userId, object notification);
}