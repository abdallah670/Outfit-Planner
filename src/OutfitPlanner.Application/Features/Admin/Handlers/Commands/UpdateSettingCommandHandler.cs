using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateSettingCommandHandler> _logger;

    public UpdateSettingCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, ILogger<UpdateSettingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.Repository<SystemSetting>()
            .GetFirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);
            
        if (setting == null)
            return Result.Failure("Setting not found");
            
        if (!setting.IsEditable)
            return Result.Failure("Setting is not editable");
        
        var oldValue = setting.Value;
        setting.Value = request.Value;
        
        await _unitOfWork.CompleteAsync();
        
        // Log audit
        await _mediator.Send(new CreateAuditLogCommand(
                "UpdateSetting",
                $"Updated setting {setting.Key} to {setting.Value}",
                "System"
            ), cancellationToken);
        
        _logger.LogInformation("Setting {Key} updated from {OldValue} to {NewValue}", request.Key, oldValue, request.Value);
        return Result.Success();
    }
}
