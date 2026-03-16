using MediatR;
using OutfitPlanner.Application.DTOs.Notification;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Queries;

public class GetNotificationsQuery : IRequest<List<NotificationDto>>
{
    public string UserId { get; set; } = string.Empty;
}
