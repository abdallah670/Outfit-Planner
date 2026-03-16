using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class DeleteStyleRuleCommandHandler : IRequestHandler<DeleteStyleRuleCommand, BaseCommandResponse>
{
    private readonly IStyleRuleRepository _styleRuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStyleRuleCommandHandler(
        IStyleRuleRepository styleRuleRepository,
        IUnitOfWork unitOfWork)
    {
        _styleRuleRepository = styleRuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeleteStyleRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _styleRuleRepository.GetByIdAsync(request.RuleId);
        
        if (rule == null)
        {
            throw new NotFoundException("StyleRule", request.RuleId);
        }

        await _styleRuleRepository.RemoveAsync(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Style rule deleted successfully"
        };
    }
}
