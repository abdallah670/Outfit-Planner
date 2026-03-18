using MediatR;
using OutfitPlanner.Application.DTOs.User;

namespace OutfitPlanner.Application.Features.User.Requests.Queries;

public class GetNotificationSettingsQuery : IRequest<NotificationSettingsDto>
{
    public string UserId { get; set; } = string.Empty;
}
