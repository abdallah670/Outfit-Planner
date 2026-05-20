using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;
using System.Text.Json;

namespace OutfitPlanner.Infrastructure.Services;

public class UserActivityService : IUserActivityService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserActivityService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public UserActivityService(
        AppDbContext dbContext,
        ILogger<UserActivityService> logger,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task RecordLoginAsync(string userId, string userName, string ipAddress, string? userAgent = null)
    {
        try
        {
            _logger.LogInformation("Recording login for user: {UserId} from IP: {IpAddress}", userId, ipAddress);

            // Create login activity record
            var activity = new OutfitPlanner.Domain.Entities.UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                UserName = userName,
                Type = ActivityType.Login,
                Description = "User logged in",
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _dbContext.UserActivities.AddAsync(activity);

            // Update user session info in cache
            var sessionInfo = new UserSessionInfo(
                userId,
                userName,
                DateTime.UtcNow,
                DateTime.UtcNow,
                TimeSpan.Zero,
                ipAddress,
                userAgent,
                0,
                0,
                "Dashboard"
            );

            var cacheKey = $"UserSession_{userId}";
            _cache.Set(cacheKey, sessionInfo, TimeSpan.FromHours(24));

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully recorded login for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record login for user: {UserId}", userId);
            throw;
        }
    }

    public async Task RecordLogoutAsync(string userId, string userName, string ipAddress)
    {
        try
        {
            _logger.LogInformation("Recording logout for user: {UserId}", userId);

            // Create logout activity record
            var activity = new OutfitPlanner.Domain.Entities.UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                UserName = userName,
                Type = ActivityType.Logout,
                Description = "User logged out",
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            await _dbContext.UserActivities.AddAsync(activity);

            // Update session info and calculate session duration
            var cacheKey = $"UserSession_{userId}";
            if (_cache.TryGetValue(cacheKey, out UserSessionInfo? sessionInfo) && sessionInfo != null)
            {
                var updatedSessionInfo = sessionInfo with
                {
                    LastActivity = DateTime.UtcNow,
                    SessionDuration = DateTime.UtcNow - sessionInfo.LoginTime
                };

                _cache.Set(cacheKey, updatedSessionInfo, TimeSpan.FromMinutes(30));
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully recorded logout for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record logout for user: {UserId}", userId);
            throw;
        }
    }

    public async Task RecordActivityAsync(string userId, string userName, string activityType, string description, string ipAddress, string? additionalData = null)
    {
        try
        {
            var type = ParseActivityType(activityType);

            var activity = new OutfitPlanner.Domain.Entities.UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                UserName = userName,
                Type = type,
                Description = description,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                AdditionalData = additionalData
            };

            await _dbContext.UserActivities.AddAsync(activity);

            // Update session info
            var cacheKey = $"UserSession_{userId}";
            if (_cache.TryGetValue(cacheKey, out UserSessionInfo? sessionInfo) && sessionInfo != null)
            {
                var updatedSessionInfo = sessionInfo with
                {
                    LastActivity = DateTime.UtcNow,
                    SessionDuration = DateTime.UtcNow - sessionInfo.LoginTime,
                    PageViews = type == ActivityType.PageView ? sessionInfo.PageViews + 1 : sessionInfo.PageViews,
                    ActionsPerformed = sessionInfo.ActionsPerformed + 1,
                    CurrentPage = type == ActivityType.PageView ? description : sessionInfo.CurrentPage
                };

                _cache.Set(cacheKey, updatedSessionInfo, TimeSpan.FromHours(24));
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record activity for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserActivityStatistics> GetUserActivityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var activities = await _dbContext.UserActivities
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .ToListAsync();

            var logins = activities.Where(a => a.Type == ActivityType.Login).ToList();
            var logouts = activities.Where(a => a.Type == ActivityType.Logout).ToList();
            var uniqueUsers = activities.Select(a => a.UserId).Distinct().Count();

            var activeUsers = await GetActiveUsersCountAsync();
            
            // Calculate average session duration
            var sessionDurations = new List<TimeSpan>();
            foreach (var user in activities.Select(a => a.UserId).Distinct())
            {
                var userLogins = logins.Where(l => l.UserId == user).OrderBy(l => l.Timestamp);
                var userLogouts = logouts.Where(l => l.UserId == user).OrderBy(l => l.Timestamp);

                for (int i = 0; i < Math.Min(userLogins.Count(), userLogouts.Count()); i++)
                {
                    sessionDurations.Add(userLogouts.ElementAt(i).Timestamp - userLogins.ElementAt(i).Timestamp);
                }
            }

            var avgSessionDuration = sessionDurations.Any() 
                ? TimeSpan.FromTicks((long)sessionDurations.Average(d => d.Ticks))
                : TimeSpan.Zero;

            // Activities by type
            var activitiesByType = activities
                .GroupBy(a => a.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Logins by hour
            var loginsByHour = logins
                .GroupBy(l => l.Timestamp.Hour)
                .ToDictionary(g => g.Key.ToString("00"), g => g.Count());

            // Logins by day of week
            var loginsByDayOfWeek = logins
                .GroupBy(l => l.Timestamp.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return new UserActivityStatistics(
                start,
                end,
                logins.Count,
                logouts.Count,
                uniqueUsers,
                activeUsers,
                avgSessionDuration.TotalMinutes,
                activities.Count,
                activitiesByType,
                loginsByHour,
                loginsByDayOfWeek
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activity statistics");
            throw;
        }
    }

    public async Task<IEnumerable<OutfitPlanner.Application.Contracts.Infrastructure.UserActivity>> GetRecentActivitiesAsync(int limit = 50, string? userId = null)
    {
        try
        {
            var query = _dbContext.UserActivities.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .Select(a => new OutfitPlanner.Application.Contracts.Infrastructure.UserActivity(
                    a.Id,
                    a.UserId,
                    a.UserName,
                    a.Type,
                    a.Description,
                    a.Timestamp,
                    a.IpAddress,
                    a.UserAgent,
                    a.AdditionalData
                ))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent activities");
            throw;
        }
    }

    public async Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(string userId, int limit = 100)
    {
        try
        {
            var activities = await _dbContext.UserActivities
                .Where(a => a.UserId == userId && (a.Type == ActivityType.Login || a.Type == ActivityType.Logout))
                .OrderByDescending(a => a.Timestamp)
                .Take(limit * 2) // Get more to pair login/logout
                .ToListAsync();

            var loginHistory = new List<UserLoginHistory>();
            var loginStack = new Stack<OutfitPlanner.Domain.Entities.UserActivity>();

            foreach (var activity in activities.OrderByDescending(a => a.Timestamp))
            {
                if (activity.Type == ActivityType.Login)
                {
                    loginStack.Push(activity);
                }
                else if (activity.Type == ActivityType.Logout && loginStack.Any())
                {
                    var login = loginStack.Pop();
                    loginHistory.Add(new UserLoginHistory(
                        login.Id,
                        login.UserId,
                        login.UserName,
                        login.Timestamp,
                        activity.Timestamp,
                        activity.Timestamp - login.Timestamp,
                        login.IpAddress,
                        login.UserAgent,
                        false
                    ));
                }
            }

            // Add unmatched logins as active sessions
            foreach (var login in loginStack)
            {
                loginHistory.Add(new UserLoginHistory(
                    login.Id,
                    login.UserId,
                    login.UserName,
                    login.Timestamp,
                    null,
                    DateTime.UtcNow - login.Timestamp,
                    login.IpAddress,
                    login.UserAgent,
                    true
                ));
            }

            return loginHistory
                .OrderByDescending(h => h.LoginTime)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get login history for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<ActiveUser>> GetActiveUsersAsync()
    {
        try
        {
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            
            var recentActivities = await _dbContext.UserActivities
                .Where(a => a.Timestamp >= thirtyMinutesAgo)
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.First().UserName,
                    LastActivity = g.Max(a => a.Timestamp),
                    IpAddress = g.OrderByDescending(a => a.Timestamp).First().IpAddress
                })
                .ToListAsync();

            var activeUsers = new List<ActiveUser>();

            foreach (var recent in recentActivities)
            {
                var cacheKey = $"UserSession_{recent.UserId}";
                if (_cache.TryGetValue(cacheKey, out UserSessionInfo? sessionInfo) && sessionInfo != null)
                {
                    activeUsers.Add(new ActiveUser(
                        recent.UserId,
                        recent.UserName,
                        recent.LastActivity,
                        recent.IpAddress,
                        sessionInfo.SessionDuration,
                        sessionInfo.CurrentPage
                    ));
                }
                else
                {
                    activeUsers.Add(new ActiveUser(
                        recent.UserId,
                        recent.UserName,
                        recent.LastActivity,
                        recent.IpAddress,
                        DateTime.UtcNow - recent.LastActivity
                    ));
                }
            }

            return activeUsers.OrderByDescending(u => u.LastActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active users");
            throw;
        }
    }

    public async Task<UserSessionInfo?> GetUserSessionInfoAsync(string userId)
    {
        try
        {
            var cacheKey = $"UserSession_{userId}";
            
            if (_cache.TryGetValue(cacheKey, out UserSessionInfo? sessionInfo) && sessionInfo != null)
            {
                return sessionInfo;
            }

            // If not in cache, try to reconstruct from database
            var lastActivity = await _dbContext.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync();

            if (lastActivity == null)
            {
                return null;
            }

            var userActivities = await _dbContext.UserActivities
                .Where(a => a.UserId == userId && a.Timestamp >= lastActivity.Timestamp.AddHours(-24))
                .ToListAsync();

            var sessionDuration = DateTime.UtcNow - lastActivity.Timestamp;
            var pageViews = userActivities.Count(a => a.Type == ActivityType.PageView);
            var actionsPerformed = userActivities.Count;

            return new UserSessionInfo(
                userId,
                lastActivity.UserName,
                lastActivity.Timestamp,
                lastActivity.Timestamp,
                sessionDuration,
                lastActivity.IpAddress,
                lastActivity.UserAgent,
                pageViews,
                actionsPerformed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session info for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserActivityTrends> GetUserActivityTrendsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var activities = await _dbContext.UserActivities
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .ToListAsync();

            // Daily counts
            var dailyCounts = activities
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new DailyActivityCount(
                    g.Key,
                    g.Count(a => a.Type == ActivityType.Login),
                    g.Count(a => a.Type == ActivityType.Logout),
                    g.Count(),
                    g.Select(a => a.UserId).Distinct().Count()
                ))
                .OrderBy(d => d.Date)
                .ToList();

            // Hourly counts
            var hourlyCounts = activities
                .GroupBy(a => a.Timestamp.Hour)
                .Select(g => new HourlyActivityCount(
                    g.Key,
                    g.Count(),
                    g.Select(a => a.UserId).Distinct().Count()
                ))
                .OrderBy(h => h.Hour)
                .ToList();

            // Activity type counts
            var activityTypeCounts = activities
                .GroupBy(a => a.Type)
                .Select(g => new ActivityTypeCount(
                    g.Key.ToString(),
                    g.Count(),
                    activities.Any() ? (double)g.Count() / activities.Count() * 100 : 0
                ))
                .OrderByDescending(a => a.Count)
                .ToList();

            return new UserActivityTrends(
                startDate,
                endDate,
                dailyCounts,
                hourlyCounts,
                activityTypeCounts,
                activities.Select(a => a.UserId).Distinct().Count(),
                activities.Count
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activity trends");
            throw;
        }
    }

    public async Task CleanupOldActivitiesAsync(int retentionDays = 90)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            
            var oldActivities = await _dbContext.UserActivities
                .Where(a => a.Timestamp < cutoffDate)
                .ToListAsync();

            if (oldActivities.Any())
            {
                _dbContext.UserActivities.RemoveRange(oldActivities);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Cleaned up {Count} old activity records older than {Days} days", 
                    oldActivities.Count, retentionDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old activities");
            throw;
        }
    }

    public async Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var allActivities = await _dbContext.UserActivities.ToListAsync();
            
            var todayActivities = allActivities.Where(a => a.Timestamp >= todayStart).ToList();
            var weekActivities = allActivities.Where(a => a.Timestamp >= weekStart).ToList();
            var monthActivities = allActivities.Where(a => a.Timestamp >= monthStart).ToList();

            var todayLogins = todayActivities.Count(a => a.Type == ActivityType.Login);
            var weekLogins = weekActivities.Count(a => a.Type == ActivityType.Login);
            var monthLogins = monthActivities.Count(a => a.Type == ActivityType.Login);

            var activeUsersToday = todayActivities.Select(a => a.UserId).Distinct().Count();
            var activeUsersThisWeek = weekActivities.Select(a => a.UserId).Distinct().Count();
            var activeUsersThisMonth = monthActivities.Select(a => a.UserId).Distinct().Count();

            // Calculate average session duration
            var sessionDurations = new List<TimeSpan>();
            var userSessions = allActivities
                .Where(a => a.Type == ActivityType.Login || a.Type == ActivityType.Logout)
                .GroupBy(a => a.UserId)
                .ToList();

            foreach (var userGroup in userSessions)
            {
                var userLogins = userGroup.Where(a => a.Type == ActivityType.Login).OrderBy(a => a.Timestamp);
                var userLogouts = userGroup.Where(a => a.Type == ActivityType.Logout).OrderBy(a => a.Timestamp);

                for (int i = 0; i < Math.Min(userLogins.Count(), userLogouts.Count()); i++)
                {
                    sessionDurations.Add(userLogouts.ElementAt(i).Timestamp - userLogins.ElementAt(i).Timestamp);
                }
            }

            var avgSessionDuration = sessionDurations.Any() 
                ? TimeSpan.FromTicks((long)sessionDurations.Average(d => d.Ticks))
                : TimeSpan.Zero;

            // Most active hour and day
            var mostActiveHour = allActivities
                .GroupBy(a => a.Timestamp.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? 0;

            var mostActiveDay = allActivities
                .GroupBy(a => a.Timestamp.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key.ToString() ?? "Unknown";

            // Top active users
            var topActiveUsers = allActivities
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.First().UserName,
                    ActivityCount = g.Count(),
                    TotalTime = CalculateUserTotalTime(g.ToList())
                })
                .OrderByDescending(u => u.ActivityCount)
                .Take(10)
                .Select(u => new TopUserActivity(u.UserId, u.UserName, u.ActivityCount, u.TotalTime))
                .ToList();

            // Popular activities
            var popularActivities = allActivities
                .GroupBy(a => a.Type)
                .Select(g => new PopularActivity(
                    g.Key.ToString(),
                    g.Count(),
                    allActivities.Any() ? (double)g.Count() / allActivities.Count() * 100 : 0
                ))
                .OrderByDescending(a => a.Count)
                .ToList();

            return new UserActivityAnalytics(
                now,
                allActivities.Select(a => a.UserId).Distinct().Count(),
                activeUsersToday,
                activeUsersThisWeek,
                activeUsersThisMonth,
                todayLogins,
                weekLogins,
                monthLogins,
                avgSessionDuration.TotalMinutes,
                mostActiveHour,
                mostActiveDay,
                topActiveUsers,
                popularActivities
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user activity analytics");
            throw;
        }
    }

    private async Task<int> GetActiveUsersCountAsync()
    {
        var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
        return await _dbContext.UserActivities
            .Where(a => a.Timestamp >= thirtyMinutesAgo)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();
    }

    private ActivityType ParseActivityType(string activityType)
    {
        return activityType.ToLowerInvariant() switch
        {
            "login" => ActivityType.Login,
            "logout" => ActivityType.Logout,
            "pageview" => ActivityType.PageView,
            "createoutfit" => ActivityType.CreateOutfit,
            "editoutfit" => ActivityType.EditOutfit,
            "deleteoutfit" => ActivityType.DeleteOutfit,
            "likeoutfit" => ActivityType.LikeOutfit,
            "comment" => ActivityType.Comment,
            "follow" => ActivityType.Follow,
            "unfollow" => ActivityType.Unfollow,
            "uploadimage" => ActivityType.UploadImage,
            "search" => ActivityType.Search,
            "viewprofile" => ActivityType.ViewProfile,
            "editprofile" => ActivityType.EditProfile,
            "adminaction" => ActivityType.AdminAction,
            _ => ActivityType.Other
        };
    }

    private TimeSpan CalculateUserTotalTime(List<OutfitPlanner.Domain.Entities.UserActivity> userActivities)
    {
        var logins = userActivities.Where(a => a.Type == ActivityType.Login).OrderBy(a => a.Timestamp);
        var logouts = userActivities.Where(a => a.Type == ActivityType.Logout).OrderBy(a => a.Timestamp);

        var totalTime = TimeSpan.Zero;
        for (int i = 0; i < Math.Min(logins.Count(), logouts.Count()); i++)
        {
            totalTime += logouts.ElementAt(i).Timestamp - logins.ElementAt(i).Timestamp;
        }

        return totalTime;
    }
}
