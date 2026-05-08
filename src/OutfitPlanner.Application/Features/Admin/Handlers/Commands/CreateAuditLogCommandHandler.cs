using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAuditLogCommandHandler> _logger;

    public CreateAuditLogCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateAuditLogCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            UserName = request.UserName,
            Action = request.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            IpAddress = request.IpAddress,
            Timestamp = DateTimeOffset.UtcNow
        };
        
        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }
}
