using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UpdateStyleRuleCommandHandler : IRequestHandler<UpdateStyleRuleCommand, BaseCommandResponse>
{
    private readonly IStyleRuleRepository _styleRuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStyleRuleCommandHandler(
        IStyleRuleRepository styleRuleRepository,
        IUnitOfWork unitOfWork)
    {
        _styleRuleRepository = styleRuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(UpdateStyleRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _styleRuleRepository.GetByIdAsync(request.RuleId);
        
        if (rule == null)
        {
            throw new NotFoundException("StyleRule", request.RuleId);
        }

        rule.Name = request.Rule.Name;
        rule.Description = request.Rule.Description;
        rule.IsActive = request.Rule.IsActive;
        rule.CriteriaJson = request.Rule.CriteriaJson;

        await _styleRuleRepository.UpdateAsync(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Style rule updated successfully"
        };
    }
}
