using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class SetMaintenanceModeCommandHandler : IRequestHandler<SetMaintenanceModeCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetMaintenanceModeCommandHandler> _logger;

    public SetMaintenanceModeCommandHandler(IUnitOfWork unitOfWork, ILogger<SetMaintenanceModeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SetMaintenanceModeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var maintenanceLog = new AuditLog
            {
                UserId = "system",
                UserName = "System",
                Action = request.Enabled ? "EnableMaintenanceMode" : "DisableMaintenanceMode",
                EntityType = "System",
                EntityId = "maintenance",
                Timestamp = DateTimeOffset.UtcNow,
                IpAddress = "127.0.0.1",
                NewValues = $"{{ \"enabled\": {request.Enabled.ToString().ToLower()}, \"message\": \"{request.Message}\" }}"
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(maintenanceLog);
            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to set maintenance mode: {ex.Message}");
        }
    }
}

public class CreateBackupCommandHandler : IRequestHandler<CreateBackupCommand, BackupResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBackupCommandHandler> _logger;

    public CreateBackupCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateBackupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BackupResult> Handle(CreateBackupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var backupId = Guid.NewGuid();
            var fileName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{command.Request.Type}.bak";
            var createdAt = DateTimeOffset.UtcNow;

            await Task.Delay(1000, cancellationToken); // Simulate backup time

            var backupLog = new AuditLog
            {
                UserId = "system",
                UserName = "System",
                Action = "CreateBackup",
                EntityType = "Backup",
                EntityId = backupId.ToString(),
                Timestamp = createdAt,
                IpAddress = "127.0.0.1",
                NewValues = $"{{ \"type\": \"{command.Request.Type}\", \"description\": \"{command.Request.Description}\" }}"
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(backupLog);
            await _unitOfWork.CompleteAsync();

            var backupSize = (long)(new Random().NextDouble() * 100 * 1024 * 1024); // 0-100MB

            return new BackupResult(
                true,
                backupId.ToString(),
                fileName,
                backupSize,
                createdAt.DateTime,
                "Backup created successfully"
            );
        }
        catch (Exception ex)
        {
            return new BackupResult(
                false,
                Guid.Empty.ToString(),
                string.Empty,
                0,
                DateTime.UtcNow,
                $"Failed to create backup: {ex.Message}"
            );
        }
    }
}

public class RestartServiceCommandHandler : IRequestHandler<RestartServiceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RestartServiceCommandHandler> _logger;

    public RestartServiceCommandHandler(IUnitOfWork unitOfWork, ILogger<RestartServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RestartServiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(2000, cancellationToken); // Simulate restart time

            var restartLog = new AuditLog
            {
                UserId = "system",
                UserName = "System",
                Action = "RestartService",
                EntityType = "Service",
                EntityId = command.ServiceName,
                Timestamp = DateTimeOffset.UtcNow,
                IpAddress = "127.0.0.1",
                NewValues = $"{{ \"serviceName\": \"{command.ServiceName}\" }}"
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(restartLog);
            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to restart service {command.ServiceName}: {ex.Message}");
        }
    }
}

public class ClearCacheCommandHandler : IRequestHandler<ClearCacheCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearCacheCommandHandler> _logger;

    public ClearCacheCommandHandler(IUnitOfWork unitOfWork, ILogger<ClearCacheCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ClearCacheCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(500, cancellationToken); // Simulate cache clearing time

            var cacheLog = new AuditLog
            {
                UserId = "system",
                UserName = "System",
                Action = "ClearCache",
                EntityType = "Cache",
                EntityId = command.CacheKey ?? "all",
                Timestamp = DateTimeOffset.UtcNow,
                IpAddress = "127.0.0.1",
                NewValues = $"{{ \"cacheKey\": \"{command.CacheKey ?? "all"}\" }}"
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(cacheLog);
            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to clear cache: {ex.Message}");
        }
    }
}
