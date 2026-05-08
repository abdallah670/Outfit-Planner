namespace OutfitPlanner.Application.DTOs.Admin;

public record ExportAnalyticsRequest(
    string Format, // "csv", "json", "pdf"
    DateTime? StartDate,
    DateTime? EndDate
);

public record SetMaintenanceModeRequest(
    bool Enabled,
    string? Message
);

public record ClearCacheRequest(
    string? CacheKey
);

public record RealtimeAnalyticsDto(
    int ActiveUsers,
    int CurrentOutfitViews,
    int CurrentPollVotes,
    double SystemLoad,
    DateTime LastUpdated
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
    int TotalLikes
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

public record SystemPerformanceMetrics(
    double CpuUsage,
    double MemoryUsage,
    double DiskUsage,
    int ActiveConnections,
    double ResponseTime,
    List<PerformanceData> HistoricalPerformance
);

public record PerformanceData(
    DateTime Timestamp,
    double CpuUsage,
    double MemoryUsage,
    double DiskUsage,
    int ActiveConnections,
    double ResponseTime
);

public record DetailedAnalyticsDto(
    UserEngagementMetrics UserMetrics,
    ContentMetrics ContentMetrics,
    SystemPerformanceMetrics SystemMetrics,
    List<TimeSeriesData> TimeSeriesData,
    AnalyticsSummary Summary
);

public record TimeSeriesData(
    DateTime Timestamp,
    double Value,
    string MetricType
);

public record AnalyticsSummary(
    int TotalUsers,
    int ActiveUsers,
    double EngagementRate,
    int TotalContent,
    double SystemHealth
);

public record ContentPerformanceData(
    Guid Id,
    string Name,
    string Type,
    int Views,
    int LikesCount,
    int CommentsCount,
    double EngagementScore
);

public record ContentTypeStats(
    string Type,
    int Count,
    int TotalEngagement,
    double AverageEngagement
);
