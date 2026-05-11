using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Queries;

public record GetAuditLogsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? UserId = null,
    string? Action = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<PaginatedResult<AuditLog>>;

public record GetAuditLogDetailsQuery(Guid Id) : IRequest<AuditLog?>;

public record GetAuditLogStatisticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<AuditLogStatistics>;

public record GetAuditLogAnalyticsQuery : IRequest<AuditLogAnalytics>;

public record GetAuditLogTrendsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<AuditLogTrends>;
