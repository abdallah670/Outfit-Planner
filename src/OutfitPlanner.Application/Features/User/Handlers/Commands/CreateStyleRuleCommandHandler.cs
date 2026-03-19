using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class CreateStyleRuleCommandHandler : IRequestHandler<CreateStyleRuleCommand, StyleRuleDto>
{
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IStyleRuleRepository _styleRuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStyleRuleCommandHandler(
        IUserStyleProfileRepository styleProfileRepository,
        IStyleRuleRepository styleRuleRepository,
        IUnitOfWork unitOfWork)
    {
        _styleProfileRepository = styleProfileRepository;
        _styleRuleRepository = styleRuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StyleRuleDto> Handle(CreateStyleRuleCommand request, CancellationToken cancellationToken)
    {
        var profile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);
        
        if (profile == null)
        {
            // Create profile if it doesn't exist
            profile = new UserStyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Style = StylePreference.Classic,
                PreferredColors = new List<string>(),
                FitPreferences = string.Empty,
                ComfortPriority = 50,
                AcceptsTrends = false
            };
            await _styleProfileRepository.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var rule = new StyleRule
        {
            Id = Guid.NewGuid(),
            UserStyleProfileId = profile.Id,
            Name = request.Rule.Name,
            Description = request.Rule.Description,
            CriteriaJson = request.Rule.CriteriaJson,
            IsActive = true
        };

        // Use the StyleRuleRepository to add the new rule
        await _styleRuleRepository.AddAsync(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the created rule as DTO
        return new StyleRuleDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Description = rule.Description,
            CriteriaJson = rule.CriteriaJson,
            IsActive = rule.IsActive
        };
    }
}
