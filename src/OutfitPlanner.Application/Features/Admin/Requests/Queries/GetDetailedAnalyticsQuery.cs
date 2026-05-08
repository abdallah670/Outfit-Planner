using MediatR;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetDetailedAnalyticsQuery(AnalyticsFilterRequest Filter) : IRequest<DetailedAnalyticsDto>;

public record AnalyticsFilterRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? ContentType = null,
    string? MetricType = null
);

// Duplicate DTO classes for GetDetailedAnalyticsQueryHandler compatibility
public record DetailedAnalyticsDto(
    UserEngagementMetrics UserMetrics,
    ContentMetrics ContentStats,
    SystemPerformanceMetrics SystemStats,
    List<TimeSeriesData> Trends,
    List<AnalyticsSummary> Summaries
);

public record UserEngagementMetrics(
    int TotalUsers,
    int ActiveUsers,
    int NewUsers,
    double UserGrowthRate,
    List<UserActivityData> DailyActivity,
    List<UserDemographics> Demographics
);

public record ContentMetrics(
    int TotalOutfits,
    int TotalPosts,
    int TotalPolls,
    int TotalComments,
    int TotalLikes,
    double EngagementRate,
    List<ContentPerformanceData> TopContent,
    List<ContentTypeStats> ContentTypeBreakdown
);

public record SystemPerformanceMetrics(
    double CpuUsage,
    long MemoryUsage,
    long DiskUsage,
    int ActiveConnections,
    double ResponseTime,
    List<PerformanceData> HistoricalPerformance
);

public record TimeSeriesData(
    DateTime Date,
    string Metric,
    double Value
);

public record AnalyticsSummary(
    string Category,
    string Metric,
    double Value,
    double ChangePercentage
);

public record UserActivityData(
    DateTime Date,
    int ActiveUsers,
    int NewUsers,
    int ReturningUsers
);

public record UserDemographics(
    string Category,
    string Value,
    int Count,
    double Percentage
);

public record ContentPerformanceData(
    Guid Id,
    string Title,
    string Type,
    int Views,
    int Likes,
    int Comments,
    double EngagementScore
);

public record ContentTypeStats(
    string Type,
    int Count,
    int TotalEngagement,
    double AverageEngagement
);

public record PerformanceData(
    DateTime Timestamp,
    double CpuUsage,
    long MemoryUsage,
    double ResponseTime
);
