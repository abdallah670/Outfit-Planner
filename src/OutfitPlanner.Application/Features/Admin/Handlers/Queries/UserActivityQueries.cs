using MediatR;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Queries;

public record GetUserActivitiesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? UserId = null,
    ActivityType? ActivityType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<PaginatedResult<OutfitPlanner.Application.Contracts.Infrastructure.UserActivity>>;

public record GetUserLoginHistoryQuery(
    string UserId,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<UserLoginHistory>>;

public record GetUserActivityStatisticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<UserActivityStatistics>;

public record GetActiveUsersQuery : IRequest<IEnumerable<ActiveUser>>;

public record GetUserSessionInfoQuery(string UserId) : IRequest<UserSessionInfo?>;

public record GetUserActivityAnalyticsQuery : IRequest<UserActivityAnalytics>;

public record GetUserActivityTrendsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<UserActivityTrends>;
