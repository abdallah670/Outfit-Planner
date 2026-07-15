using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;

namespace OutfitPlanner.Application.Features.Notifications;

/// <summary>
/// Convenience function to notify mentioned/tagged users.
/// Wraps <see cref="NotifyMentionedUsersCommand"/> so callers can write a single line.
/// </summary>
public static class NotificationMediatorExtensions
{
    public static Task NotifyMentionedUsersAsync(
        this IMediator mediator,
        List<string> userIds,
        string actorUserId,
        string contentSnippet,
        string actionUrl,
        string title = "New mention",
        string verb = "mentioned",
        string? actorName = null,
        CancellationToken cancellationToken = default)
        => mediator.Send(new NotifyMentionedUsersCommand
        {
            UserIds = userIds,
            ActorUserId = actorUserId,
            ActorName = actorName,
            ContentSnippet = contentSnippet,
            ActionUrl = actionUrl,
            Title = title,
            Verb = verb
        }, cancellationToken);
}
