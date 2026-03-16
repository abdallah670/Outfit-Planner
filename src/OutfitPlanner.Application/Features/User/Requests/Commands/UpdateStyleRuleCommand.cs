using MediatR;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class UpdateStyleRuleCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid RuleId { get; set; }
    public UpdateStyleRuleDto Rule { get; set; } = null!;
}
