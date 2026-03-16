using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Commands;

public class MarkAllAsReadCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
}
