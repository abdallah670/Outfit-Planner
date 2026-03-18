using MediatR;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.Features.User.Requests.Commands;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class DeleteAccountCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
}
