using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedResult<AuditLog>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAuditLogsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AuditLog>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting audit logs - Page: {PageNumber}, PageSize: {PageSize}, UserId: {UserId}",
                request.PageNumber, request.PageSize, request.UserId);

            var auditLogs = _unitOfWork.AuditLogs.GetQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.UserId))
            {
                auditLogs = auditLogs.Where(a => a.UserId == request.UserId);
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                auditLogs = auditLogs.Where(a => a.Action.Contains(request.Action));
            }

            if (request.StartDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.Timestamp >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.Timestamp <= request.EndDate.Value);
            }

            // Order by timestamp descending
            auditLogs = auditLogs.OrderByDescending(a => a.Timestamp);

            // Apply pagination
            var totalCount = await _unitOfWork.AuditLogs.CountAsync();
            var items = auditLogs
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedResult<AuditLog>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs");
            throw;
        }
    }
}

public class GetAuditLogDetailsQueryHandler : IRequestHandler<GetAuditLogDetailsQuery, AuditLog?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditLogDetailsQueryHandler> _logger;

    public GetAuditLogDetailsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAuditLogDetailsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuditLog?> Handle(GetAuditLogDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting audit log details for ID: {Id}", request.Id);

            var auditLog = await _unitOfWork.AuditLogs.GetByIdAsync(request.Id);
            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log details for ID: {Id}", request.Id);
            throw;
        }
    }
}

public class GetAuditLogStatisticsQueryHandler : IRequestHandler<GetAuditLogStatisticsQuery, AuditLogStatistics>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditLogStatisticsQueryHandler> _logger;

    public GetAuditLogStatisticsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAuditLogStatisticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuditLogStatistics> Handle(GetAuditLogStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting audit log statistics");

            var auditLogs = _unitOfWork.AuditLogs.GetQueryable();

            if (request.StartDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.Timestamp >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.Timestamp <= request.EndDate.Value);
            }

            var totalLogs = await _unitOfWork.AuditLogs.CountAsync();
            var uniqueUsers = auditLogs.Select(a => a.UserId).Distinct().Count();
            var topActions = auditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var logsByHour = auditLogs
                .GroupBy(a => new { a.Timestamp.Date, a.Timestamp.Hour })
                .Select(g => new { Hour = g.Key.Hour, Count = g.Count() })
                .OrderBy(x => x.Hour)
                .ToList();

            return new AuditLogStatistics
            {
                TotalLogs = totalLogs,
                UniqueUsers = uniqueUsers,
                TopActions = topActions.Select(x => x.Action).ToList(),
                LogsByHour = logsByHour.ToDictionary(x => x.Hour, x => x.Count)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log statistics");
            throw;
        }
    }
}

public class GetAuditLogAnalyticsQueryHandler : IRequestHandler<GetAuditLogAnalyticsQuery, AuditLogAnalytics>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditLogAnalyticsQueryHandler> _logger;

    public GetAuditLogAnalyticsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAuditLogAnalyticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuditLogAnalytics> Handle(GetAuditLogAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting audit log analytics");

            var auditLogs = _unitOfWork.AuditLogs.GetQueryable();
            var last24Hours = DateTime.UtcNow.AddHours(-24);
            
            var recentLogs = auditLogs.Where(a => a.Timestamp >= last24Hours);
            var totalRecentLogs = await _unitOfWork.AuditLogs.CountAsync();
            var recentUsers = recentLogs.Select(a => a.UserId).Distinct().Count();
            
            var actionBreakdown = recentLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var userBreakdown = recentLogs
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            return new AuditLogAnalytics
            {
                TotalRecentLogs = totalRecentLogs,
                RecentUsers = recentUsers,
                ActionBreakdown = actionBreakdown.ToDictionary(x => x.Action, x => x.Count),
                TopUsers = userBreakdown.Select(x => x.UserId).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log analytics");
            throw;
        }
    }
}

public class GetAuditLogTrendsQueryHandler : IRequestHandler<GetAuditLogTrendsQuery, AuditLogTrends>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditLogTrendsQueryHandler> _logger;

    public GetAuditLogTrendsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAuditLogTrendsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuditLogTrends> Handle(GetAuditLogTrendsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting audit log trends from {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            var auditLogs = _unitOfWork.AuditLogs.GetQueryable()
                .Where(a => a.Timestamp >= request.StartDate && a.Timestamp <= request.EndDate);

            var dailyTrends = auditLogs
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            var actionTrends = auditLogs
                .GroupBy(a => new { a.Timestamp.Date, a.Action })
                .Select(g => new { Date = g.Key.Date, Action = g.Key.Action, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            return new AuditLogTrends
            {
                DailyTrends = dailyTrends.ToDictionary(x => x.Date, x => x.Count),
                ActionTrends = actionTrends.GroupBy(x => x.Action)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Date, x => x.Count))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log trends");
            throw;
        }
    }
}
