using System.Collections.Generic;

namespace OutfitPlanner.Application.Contracts.Infrastructure;

/// <summary>
/// Abstraction for pushing real-time notifications via SignalR.
/// Implemented in Infrastructure layer to avoid SignalR dependency in Application.
/// </summary>
public interface INotificationHubService
{
    Task SendNotificationAsync(string userId, object notification);

    /// <summary>
    /// Pushes a single notification to multiple users at once (one SignalR call).
    /// Each user is assumed to be in a group named by their userId.
    /// </summary>
    Task SendNotificationToUsersAsync(IEnumerable<string> userIds, object notification);
}
