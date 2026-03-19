using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class DisconnectAccountCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
