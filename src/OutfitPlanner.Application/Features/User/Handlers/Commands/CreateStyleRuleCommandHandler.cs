using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class CreateStyleRuleCommandHandler : IRequestHandler<CreateStyleRuleCommand, BaseCommandResponse>
{
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStyleRuleCommandHandler(
        IUserStyleProfileRepository styleProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _styleProfileRepository = styleProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(CreateStyleRuleCommand request, CancellationToken cancellationToken)
    {
        var profile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);
        
        if (profile == null)
        {
            // Create profile if it doesn't exist
            profile = new UserStyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Style = StylePreference.Casual,
                PreferredColors = new List<string>(),
                FitPreferences = string.Empty,
                ComfortPriority = 50,
                AcceptsTrends = false
            };
            await _styleProfileRepository.AddAsync(profile);
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

        profile.CustomRules.Add(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Style rule created successfully",
            Id = rule.Id
        };
    }
}
