using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Notifications.Handlers.Commands;

public class NotifyMentionedUsersCommandHandler : IRequestHandler<NotifyMentionedUsersCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _notificationHub;
    private readonly ILogger<NotifyMentionedUsersCommandHandler> _logger;

    public NotifyMentionedUsersCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationHubService notificationHub,
        ILogger<NotifyMentionedUsersCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task Handle(NotifyMentionedUsersCommand request, CancellationToken cancellationToken)
    {
        if (request.UserIds == null || request.UserIds.Count == 0) return;

        var actorName = request.ActorName;
        if (string.IsNullOrWhiteSpace(actorName))
        {
            var actor = await _unitOfWork.Users.GetByIdAsync(request.ActorUserId);
            actorName = actor?.Name ?? "Someone";
        }

        var snippet = string.IsNullOrWhiteSpace(request.ContentSnippet)
            ? ""
            : (request.ContentSnippet.Length > 100
                ? request.ContentSnippet.Substring(0, 100) + "..."
                : request.ContentSnippet);

        var targetIds = request.UserIds
            .Where(id => !string.IsNullOrWhiteSpace(id) && id != request.ActorUserId)
            .Distinct()
            .ToList();

        if (targetIds.Count == 0) return;

        var now = DateTimeOffset.UtcNow;
        var notifications = targetIds.Select(id => new Notification
        {
            UserId = id,
            Type = NotificationType.Social,
            Title = request.Title,
            Message = $"{actorName} {request.Verb} you: \"{snippet}\"",
            ActionUrl = request.ActionUrl,
            IsRead = false,
            CreatedAt = now
        }).ToList();

        // Bulk insert all mention notifications at once.
        await _unitOfWork.Notifications.AddRangeAsync(notifications, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Single real-time push to every mentioned user at once (one SignalR call).
        try
        {
            var dto = new NotificationDto
            {
                UserId = targetIds.Count == 1 ? targetIds[0] : string.Empty,
                Type = NotificationType.Social,
                Title = request.Title,
                Message = $"{actorName} {request.Verb} you: \"{snippet}\"",
                ActionUrl = request.ActionUrl,
                IsRead = false,
                CreatedAt = now
            };
            await _notificationHub.SendNotificationToUsersAsync(targetIds, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast mention notifications via SignalR");
        }
    }
}
