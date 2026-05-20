using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Contracts.Infrastructure;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record SetMaintenanceModeCommand(bool Enabled, string? Message) : IRequest<Result>;

public record CreateBackupCommand(
    string Name,
    string? Description = null,
    bool IncludeFiles = true,
    bool Compress = true,
    string[]? ExcludedTables = null
) : IRequest<OutfitPlanner.Application.Contracts.Infrastructure.BackupResult>;

public record RestartServiceCommand(string ServiceName) : IRequest<Result>;

public record ClearCacheCommand(string? CacheKey = null) : IRequest<Result>;
