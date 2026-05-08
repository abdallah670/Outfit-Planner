using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetSystemHealthQuery : IRequest<SystemHealthDto>;

public record GetSystemLogsQuery(SystemLogFilterRequest Filter) : IRequest<PaginatedResult<SystemLogDto>>;

public record GetSystemPerformanceQuery : IRequest<SystemPerformanceDto>;

public record SystemLogFilterRequest(
    string? Level = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 25
);

public record SystemHealthDto(
    bool DatabaseHealthy,
    bool CacheHealthy,
    bool EmailServiceHealthy,
    double CpuUsage,
    long MemoryUsage,
    long DiskUsage,
    DateTime LastCheck,
    List<string> Services,
    List<HealthCheckResult> HealthChecks
);

public record SystemLogDto(
    Guid Id,
    DateTime Timestamp,
    string Level,
    string Category,
    string Message,
    string? ExceptionDetails,
    string? UserId,
    string? UserName,
    string? IpAddress,
    string? UserAgent
);

public record SystemPerformanceDto(
    double CpuUsage,
    long MemoryUsage,
    long DiskUsage,
    int ActiveConnections,
    double AverageResponseTime,
    double RequestsPerSecond,
    List<PerformanceMetric> Metrics,
    DateTime LastUpdated
);

public record HealthCheckResult(
    string Service,
    bool Healthy,
    string Message,
    TimeSpan ResponseTime,
    DateTime LastCheck
);

public record PerformanceMetric(
    string Name,
    double Value,
    string Unit,
    DateTime Timestamp
);
