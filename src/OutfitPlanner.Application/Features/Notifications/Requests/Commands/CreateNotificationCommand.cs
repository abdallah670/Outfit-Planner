using MediatR;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Commands;

public class CreateNotificationCommand : IRequest<NotificationDto>
{
    public string UserId { get; set; } = string.Empty;
    public CreateNotificationDto Request { get; set; } = null!;
}
