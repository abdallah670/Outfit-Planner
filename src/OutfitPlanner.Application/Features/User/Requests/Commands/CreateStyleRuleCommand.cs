using MediatR;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class CreateStyleRuleCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public CreateStyleRuleDto Rule { get; set; } = null!;
}
