using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Commands;

public class DeleteNotificationCommand : IRequest<BaseCommandResponse>
{
    public Guid NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
