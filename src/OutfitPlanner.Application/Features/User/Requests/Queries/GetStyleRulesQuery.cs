using MediatR;
using OutfitPlanner.Application.DTOs.User;

namespace OutfitPlanner.Application.Features.User.Requests.Queries;

public class GetStyleRulesQuery : IRequest<List<StyleRuleDto>>
{
    public string UserId { get; set; } = string.Empty;
}
