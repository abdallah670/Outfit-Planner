using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Admin.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetUserActivitiesQueryHandler : IRequestHandler<GetUserActivitiesQuery, PaginatedResult<OutfitPlanner.Application.Contracts.Infrastructure.UserActivity>>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetUserActivitiesQueryHandler> _logger;

    public GetUserActivitiesQueryHandler(IUserActivityService userActivityService, ILogger<GetUserActivitiesQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<PaginatedResult<OutfitPlanner.Application.Contracts.Infrastructure.UserActivity>> Handle(GetUserActivitiesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting user activities - Page: {PageNumber}, PageSize: {PageSize}, UserId: {UserId}", 
                request.PageNumber, request.PageSize, request.UserId);

            // Get all activities (the service doesn't have pagination, so we'll implement it here)
            var allActivities = await _userActivityService.GetRecentActivitiesAsync(1000, request.UserId);
            
            // Apply filters
            var filteredActivities = allActivities.AsEnumerable();
            
            if (request.ActivityType.HasValue)
            {
                filteredActivities = filteredActivities.Where(a => a.Type == request.ActivityType.Value);
            }
            
            if (request.StartDate.HasValue)
            {
                filteredActivities = filteredActivities.Where(a => a.Timestamp >= request.StartDate.Value);
            }
            
            if (request.EndDate.HasValue)
            {
                filteredActivities = filteredActivities.Where(a => a.Timestamp <= request.EndDate.Value);
            }

            // Order by timestamp descending
            filteredActivities = filteredActivities.OrderByDescending(a => a.Timestamp);

            // Apply pagination
            var totalCount = filteredActivities.Count();
            var items = filteredActivities
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedResult<OutfitPlanner.Application.Contracts.Infrastructure.UserActivity>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activities");
            throw;
        }
    }
}

public class GetUserLoginHistoryQueryHandler : IRequestHandler<GetUserLoginHistoryQuery, PaginatedResult<UserLoginHistory>>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetUserLoginHistoryQueryHandler> _logger;

    public GetUserLoginHistoryQueryHandler(IUserActivityService userActivityService, ILogger<GetUserLoginHistoryQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<PaginatedResult<UserLoginHistory>> Handle(GetUserLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting login history for user: {UserId}, Page: {PageNumber}, PageSize: {PageSize}", 
                request.UserId, request.PageNumber, request.PageSize);

            // Get all login history (the service doesn't have pagination)
            var allLoginHistory = await _userActivityService.GetUserLoginHistoryAsync(request.UserId, 1000);
            
            // Order by login time descending
            allLoginHistory = allLoginHistory.OrderByDescending(h => h.LoginTime).ToList();

            // Apply pagination
            var totalCount = allLoginHistory.Count();
            var items = allLoginHistory
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedResult<UserLoginHistory>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get login history for user: {UserId}", request.UserId);
            throw;
        }
    }
}

public class GetUserActivityStatisticsQueryHandler : IRequestHandler<GetUserActivityStatisticsQuery, UserActivityStatistics>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetUserActivityStatisticsQueryHandler> _logger;

    public GetUserActivityStatisticsQueryHandler(IUserActivityService userActivityService, ILogger<GetUserActivityStatisticsQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<UserActivityStatistics> Handle(GetUserActivityStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting user activity statistics from {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            return await _userActivityService.GetUserActivityStatisticsAsync(request.StartDate, request.EndDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activity statistics");
            throw;
        }
    }
}

public class GetActiveUsersQueryHandler : IRequestHandler<GetActiveUsersQuery, IEnumerable<ActiveUser>>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetActiveUsersQueryHandler> _logger;

    public GetActiveUsersQueryHandler(IUserActivityService userActivityService, ILogger<GetActiveUsersQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<IEnumerable<ActiveUser>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting active users");

            return await _userActivityService.GetActiveUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active users");
            throw;
        }
    }
}

public class GetUserSessionInfoQueryHandler : IRequestHandler<GetUserSessionInfoQuery, UserSessionInfo?>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetUserSessionInfoQueryHandler> _logger;

    public GetUserSessionInfoQueryHandler(IUserActivityService userActivityService, ILogger<GetUserSessionInfoQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<UserSessionInfo?> Handle(GetUserSessionInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting session info for user: {UserId}", request.UserId);

            return await _userActivityService.GetUserSessionInfoAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session info for user: {UserId}", request.UserId);
            throw;
        }
    }
}

public class GetUserActivityAnalyticsQueryHandler : IRequestHandler<GetUserActivityAnalyticsQuery, UserActivityAnalytics>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetUserActivityAnalyticsQueryHandler> _logger;

    public GetUserActivityAnalyticsQueryHandler(IUserActivityService userActivityService, ILogger<GetUserActivityAnalyticsQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<UserActivityAnalytics> Handle(GetUserActivityAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting user activity analytics");

            return await _userActivityService.GetUserActivityAnalyticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activity analytics");
            throw;
        }
    }
}

public class GetUserActivityTrendsQueryHandler : IRequestHandler<GetUserActivityTrendsQuery, UserActivityTrends>
{
    private readonly IUserActivityService _userActivityService;
    private readonly ILogger<GetUserActivityTrendsQueryHandler> _logger;

    public GetUserActivityTrendsQueryHandler(IUserActivityService userActivityService, ILogger<GetUserActivityTrendsQueryHandler> logger)
    {
        _userActivityService = userActivityService;
        _logger = logger;
    }

    public async Task<UserActivityTrends> Handle(GetUserActivityTrendsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting user activity trends from {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            return await _userActivityService.GetUserActivityTrendsAsync(request.StartDate, request.EndDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activity trends");
            throw;
        }
    }
}
