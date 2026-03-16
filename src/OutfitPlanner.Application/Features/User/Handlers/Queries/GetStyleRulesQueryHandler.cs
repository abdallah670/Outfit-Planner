using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetStyleRulesQueryHandler : IRequestHandler<GetStyleRulesQuery, List<StyleRuleDto>>
{
    private readonly IUserStyleProfileRepository _styleProfileRepository;

    public GetStyleRulesQueryHandler(IUserStyleProfileRepository styleProfileRepository)
    {
        _styleProfileRepository = styleProfileRepository;
    }

    public async Task<List<StyleRuleDto>> Handle(GetStyleRulesQuery request, CancellationToken cancellationToken)
    {
        var profile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);
        
        if (profile == null)
            return new List<StyleRuleDto>();

        return profile.CustomRules.Select(r => new StyleRuleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            CriteriaJson = r.CriteriaJson
        }).ToList();
    }
}
