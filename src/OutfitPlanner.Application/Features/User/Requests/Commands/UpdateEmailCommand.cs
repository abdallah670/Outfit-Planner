using MediatR;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class UpdateEmailCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public UpdateEmailDto Request { get; set; } = new();
}
