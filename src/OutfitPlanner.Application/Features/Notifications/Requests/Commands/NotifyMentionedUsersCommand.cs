using MediatR;
using System.Collections.Generic;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Commands;

/// <summary>
/// Notifies a set of users about an activity — used both when a user is
/// mentioned in a comment and when a user is tagged in a post.
/// Notification entities are inserted in bulk via AddRangeAsync.
/// </summary>
public class NotifyMentionedUsersCommand : IRequest
{
    /// <summary>Target user IDs to notify (mentioned or tagged).</summary>
    public List<string> UserIds { get; set; } = new();

    /// <summary>The user who performed the action (excluded from recipients).</summary>
    public string ActorUserId { get; set; } = string.Empty;

    /// <summary>Display name of the actor. Resolved from the user store if not provided.</summary>
    public string? ActorName { get; set; }

    /// <summary>Short snippet of the comment/post content shown in the message.</summary>
    public string ContentSnippet { get; set; } = string.Empty;

    /// <summary>Deep link opened when the notification is tapped.</summary>
    public string ActionUrl { get; set; } = string.Empty;

    public string Title { get; set; } = "New mention";

    /// <summary>Verb used in the message, e.g. "mentioned" or "tagged".</summary>
    public string Verb { get; set; } = "mentioned";
}
