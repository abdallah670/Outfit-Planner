namespace OutfitPlanner.Application.DTOs.Admin;

public record AnalyticsFilterRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null
);


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
    UserEngagementMetrics UserEngagement,
    ContentMetrics ContentMetrics,
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
    int TotalLikes,
    double EngagementRate,
    List<ContentPerformanceData> TopContent,
    List<ContentTypeStats> ContentTypeBreakdown
);

public record UserActivityData(
    DateTime Date,
    int ActiveUsers,
    int NewUsers,
    int ReturningUsers
);

public class UserDemographics
{
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }

    public UserDemographics(string category, string value, int count, double percentage)
    {
        Category = category;
        Value = value;
        Count = count;
        Percentage = percentage;
    }
}





public record DetailedAnalyticsDto(
    UserEngagementMetrics UserMetrics,
    ContentMetrics ContentMetrics,
    List<TimeSeriesData> TimeSeriesData,
    List<AnalyticsSummary> Summary
);

public record TimeSeriesData(
    DateTime Date,
    string MetricType,
    double Value
);

public record AnalyticsSummary(
    string Category,
    string MetricName,
    int Value,
    double ChangePercentage
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
