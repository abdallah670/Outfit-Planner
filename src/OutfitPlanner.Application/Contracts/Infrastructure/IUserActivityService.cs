using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IUserActivityService
{
    /// <summary>
    /// Records a user login activity
    /// </summary>
    Task RecordLoginAsync(string userId, string userName, string ipAddress, string? userAgent = null);

    /// <summary>
    /// Records a user logout activity
    /// </summary>
    Task RecordLogoutAsync(string userId, string userName, string ipAddress);

    /// <summary>
    /// Records a user activity (page view, action, etc.)
    /// </summary>
    Task RecordActivityAsync(string userId, string userName, string activityType, string description, string ipAddress, string? additionalData = null);

    /// <summary>
    /// Gets user activity statistics
    /// </summary>
    Task<UserActivityStatistics> GetUserActivityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets recent user activities
    /// </summary>
    Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 50, string? userId = null);

    /// <summary>
    /// Gets user login history
    /// </summary>
    Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(string userId, int limit = 100);

    /// <summary>
    /// Gets currently active users (logged in within last 30 minutes)
    /// </summary>
    Task<IEnumerable<ActiveUser>> GetActiveUsersAsync();

    /// <summary>
    /// Gets user session information
    /// </summary>
    Task<UserSessionInfo?> GetUserSessionInfoAsync(string userId);

    /// <summary>
    /// Gets user activity trends over time
    /// </summary>
    Task<UserActivityTrends> GetUserActivityTrendsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Cleans up old activity records
    /// </summary>
    Task CleanupOldActivitiesAsync(int retentionDays = 90);

    /// <summary>
    /// Gets user activity analytics for dashboard
    /// </summary>
    Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync();
}

public record UserActivityStatistics(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int TotalLogins,
    int TotalLogouts,
    int UniqueUsers,
    int ActiveUsers,
    double AverageSessionDuration,
    int TotalActivities,
    Dictionary<string, int> ActivitiesByType,
    Dictionary<string, int> LoginsByHour,
    Dictionary<string, int> LoginsByDayOfWeek
);

public record UserActivity(
    string Id,
    string UserId,
    string UserName,
    ActivityType Type,
    string Description,
    DateTime Timestamp,
    string IpAddress,
    string? UserAgent = null,
    string? AdditionalData = null
);

public record UserLoginHistory(
    string Id,
    string UserId,
    string UserName,
    DateTime LoginTime,
    DateTime? LogoutTime,
    TimeSpan? SessionDuration,
    string IpAddress,
    string? UserAgent = null,
    bool IsActiveSession = false
);

public record ActiveUser(
    string UserId,
    string UserName,
    DateTime LastActivity,
    string IpAddress,
    TimeSpan SessionDuration,
    string? CurrentPage = null
);

public record UserSessionInfo(
    string UserId,
    string UserName,
    DateTime LoginTime,
    DateTime LastActivity,
    TimeSpan SessionDuration,
    string IpAddress,
    string? UserAgent = null,
    int PageViews = 0,
    int ActionsPerformed = 0,
    string? CurrentPage = null
);

public record UserActivityTrends(
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<DailyActivityCount> DailyCounts,
    IEnumerable<HourlyActivityCount> HourlyCounts,
    IEnumerable<ActivityTypeCount> ActivityTypeCounts,
    int TotalUsers,
    int TotalActivities
);

public record DailyActivityCount(
    DateTime Date,
    int Logins,
    int Logouts,
    int Activities,
    int UniqueUsers
);

public record HourlyActivityCount(
    int Hour,
    int Activities,
    int UniqueUsers
);

public record ActivityTypeCount(
    string ActivityType,
    int Count,
    double Percentage
);

public record UserActivityAnalytics(
    DateTime GeneratedAt,
    int TotalUsers,
    int ActiveUsersToday,
    int ActiveUsersThisWeek,
    int ActiveUsersThisMonth,
    int TotalLoginsToday,
    int TotalLoginsThisWeek,
    int TotalLoginsThisMonth,
    double AverageSessionDuration,
    int MostActiveHour,
    string MostActiveDay,
    IEnumerable<TopUserActivity> TopActiveUsers,
    IEnumerable<PopularActivity> PopularActivities
);

public record TopUserActivity(
    string UserId,
    string UserName,
    int ActivityCount,
    TimeSpan TotalTimeSpent
);

public record PopularActivity(
    string ActivityType,
    int Count,
    double Percentage
);

