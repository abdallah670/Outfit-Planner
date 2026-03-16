using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class DeleteStyleRuleCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid RuleId { get; set; }
}
