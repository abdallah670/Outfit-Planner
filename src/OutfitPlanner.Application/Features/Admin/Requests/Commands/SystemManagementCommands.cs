using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record SetMaintenanceModeCommand(bool Enabled, string? Message) : IRequest<Result>;

public record CreateBackupCommand(CreateBackupRequest Request) : IRequest<BackupResult>;

public record CreateBackupRequest(
    string Type, // "full", "incremental", "differential"
    string? Description = null
);

public record BackupResult(
    bool Success,
    string BackupId,
    string FileName,
    long Size,
    DateTime CreatedAt,
    string? Message = null
);

public record RestartServiceCommand(string ServiceName) : IRequest<Result>;

public record ClearCacheCommand(string? CacheKey = null) : IRequest<Result>;
