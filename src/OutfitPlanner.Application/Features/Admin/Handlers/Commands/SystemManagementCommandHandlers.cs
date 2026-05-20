using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class SetMaintenanceModeCommandHandler : IRequestHandler<SetMaintenanceModeCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<SetMaintenanceModeCommandHandler> _logger;

    public SetMaintenanceModeCommandHandler(IUnitOfWork unitOfWork, IMaintenanceService maintenanceService, ILogger<SetMaintenanceModeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    public async Task<Result> Handle(SetMaintenanceModeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Setting maintenance mode to {Enabled} with message: {Message}", 
                request.Enabled, request.Message);

            if (request.Enabled)
            {
                await _maintenanceService.EnableMaintenanceModeAsync(request.Message);
            }
            else
            {
                await _maintenanceService.DisableMaintenanceModeAsync();
            }

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

            _logger.LogInformation("Maintenance mode {Action} successfully", 
                request.Enabled ? "enabled" : "disabled");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set maintenance mode to {Enabled}", request.Enabled);
            return Result.Failure($"Failed to set maintenance mode: {ex.Message}");
        }
    }
}

public class CreateBackupCommandHandler : IRequestHandler<CreateBackupCommand, OutfitPlanner.Application.Contracts.Infrastructure.BackupResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackupService _backupService;
    private readonly ILogger<CreateBackupCommandHandler> _logger;

    public CreateBackupCommandHandler(IUnitOfWork unitOfWork, IBackupService backupService, ILogger<CreateBackupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _backupService = backupService;
        _logger = logger;
    }

    public async Task<OutfitPlanner.Application.Contracts.Infrastructure.BackupResult> Handle(CreateBackupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating backup: {Name}", command.Name);

            var backupRequest = new BackupRequest(
                command.Name,
                command.Description ?? "Manual backup",
                command.IncludeFiles,
                command.Compress,
                command.ExcludedTables
            );

            var backupResult = await _backupService.CreateBackupAsync(backupRequest);

            if (backupResult.Success)
            {
                var backupLog = new AuditLog
                {
                    UserId = "system",
                    UserName = "System",
                    Action = "CreateBackup",
                    EntityType = "Backup",
                    EntityId = backupResult.BackupId,
                    Timestamp = DateTimeOffset.UtcNow,
                    IpAddress = "127.0.0.1",
                    NewValues = $"{{ \"backupId\": \"{backupResult.BackupId}\", \"fileName\": \"{backupResult.FileName}\", \"fileSize\": {backupResult.FileSize} }}"
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(backupLog);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Backup created successfully: {BackupId}", backupResult.BackupId);
            }
            else
            {
                _logger.LogError("Failed to create backup: {Error}", backupResult.ErrorMessage);
            }

            return backupResult;
        }
        catch (Exception ex)
        {
            return new OutfitPlanner.Application.Contracts.Infrastructure.BackupResult(
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
    private readonly IServiceManagementService _serviceManagementService;
    private readonly ILogger<RestartServiceCommandHandler> _logger;

    public RestartServiceCommandHandler(IUnitOfWork unitOfWork, IServiceManagementService serviceManagementService, ILogger<RestartServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _serviceManagementService = serviceManagementService;
        _logger = logger;
    }

    public async Task<Result> Handle(RestartServiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Restarting service: {ServiceName}", command.ServiceName);

            var restartResult = await _serviceManagementService.RestartServiceAsync(command.ServiceName);

            if (restartResult.Success)
            {
                var restartLog = new AuditLog
                {
                    UserId = "system",
                    UserName = "System",
                    Action = "RestartService",
                    EntityType = "Service",
                    EntityId = restartResult.ServiceName,
                    Timestamp = DateTimeOffset.UtcNow,
                    IpAddress = "127.0.0.1",
                    NewValues = $"{{ \"serviceName\": \"{restartResult.ServiceName}\", \"restartedAt\": \"{restartResult.RestartedAt:O}\", \"downtime\": \"{restartResult.Downtime.TotalMilliseconds}ms\" }}"
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(restartLog);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Service {ServiceName} restarted successfully", restartResult.ServiceName);
                return Result.Success();
            }
            else
            {
                _logger.LogError("Failed to restart service {ServiceName}: {Error}", command.ServiceName, restartResult.ErrorMessage);
                return Result.Failure($"Failed to restart service {command.ServiceName}: {restartResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart service: {ServiceName}", command.ServiceName);
            return Result.Failure($"Failed to restart service {command.ServiceName}: {ex.Message}");
        }
    }
}

public class ClearCacheCommandHandler : IRequestHandler<ClearCacheCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheManagementService _cacheManagementService;
    private readonly ILogger<ClearCacheCommandHandler> _logger;

    public ClearCacheCommandHandler(IUnitOfWork unitOfWork, ICacheManagementService cacheManagementService, ILogger<ClearCacheCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheManagementService = cacheManagementService;
        _logger = logger;
    }

    public async Task<Result> Handle(ClearCacheCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Clearing cache for key: {CacheKey}", command.CacheKey ?? "all");

            if (string.IsNullOrEmpty(command.CacheKey))
            {
                // Clear all cache
                await _cacheManagementService.ClearAllCacheAsync();
            }
            else if (command.CacheKey.Contains("*"))
            {
                // Clear cache by pattern
                await _cacheManagementService.ClearCacheByPatternAsync(command.CacheKey);
            }
            else
            {
                // Clear specific cache key
                await _cacheManagementService.ClearCacheByKeyAsync(command.CacheKey);
            }

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

            _logger.LogInformation("Successfully cleared cache for key: {CacheKey}", command.CacheKey ?? "all");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache for key: {CacheKey}", command.CacheKey);
            return Result.Failure($"Failed to clear cache: {ex.Message}");
        }
    }
}
