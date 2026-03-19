using MediatR;
using OutfitPlanner.Application.DTOs.User;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class CreateStyleRuleCommand : IRequest<StyleRuleDto>
{
    public string UserId { get; set; } = string.Empty;
    public CreateStyleRuleDto Rule { get; set; } = null!;
}
