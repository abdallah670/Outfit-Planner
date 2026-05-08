using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetDetailedAnalyticsQueryHandler : IRequestHandler<GetDetailedAnalyticsQuery, DetailedAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDetailedAnalyticsQueryHandler> _logger;

    public GetDetailedAnalyticsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDetailedAnalyticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DetailedAnalyticsDto> Handle(GetDetailedAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.Filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.Filter.EndDate ?? DateTime.UtcNow;

        // User Engagement Metrics
        var userMetrics = await GetUserEngagementMetrics(startDate, endDate, cancellationToken);
        
        // Content Performance
        var contentMetrics = await GetContentMetrics(startDate, endDate, cancellationToken);
        
        // System Performance
        var systemMetrics = await GetSystemPerformanceMetrics(startDate, endDate, cancellationToken);
        
        // Time Series Data
        var trends = await GetTimeSeriesData(startDate, endDate, cancellationToken);
        
        // Analytics Summaries
        var summaries = await GetAnalyticsSummaries(startDate, endDate, cancellationToken);

        return new DetailedAnalyticsDto(
            userMetrics,
            contentStats,
            systemStats,
            trends,
            summaries
        );
    }

    private async Task<UserEngagementMetrics> GetUserEngagementMetrics(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var totalUsers = await _unitOfWork.Repository<User>().CountAsync();
        
        var activeUsers = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-30));

        var newUsers = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.CreatedAt >= request.Filter.StartDate && u.CreatedAt <= request.Filter.EndDate);

        var previousPeriodUsers = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.CreatedAt >= startDate.AddDays(-30) && u.CreatedAt < startDate);

        var userGrowthRate = previousPeriodUsers > 0 ? 
            ((double)(newUsers - previousPeriodUsers) / previousPeriodUsers) * 100 : 0;

        // Daily activity data
        var dailyActivity = await _unitOfWork.Repository<User>()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new UserActivityData(
                g.Key,
                g.Count(),
                g.Count(),
                0 // Returning users would need more complex logic
            ))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        // Demographics
        var demographics = new List<UserDemographics>
        {
            new("Role", "Admin", await _unitOfWork.Repository<UserRole>().CountAsync(ur => ur.Role.Name == "Admin", cancellationToken), 0),
            new("Role", "Planner", await _unitOfWork.Repository<UserRole>().CountAsync(ur => ur.Role.Name == "Planner", cancellationToken), 0)
        };

        foreach (var demo in demographics)
        {
            demo.Percentage = totalUsers > 0 ? ((double)demo.Count / totalUsers) * 100 : 0;
        }

        return new UserEngagementMetrics(
            totalUsers,
            activeUsers,
            newUsers,
            userGrowthRate,
            dailyActivity,
            demographics
        );
    }

    private async Task<ContentMetrics> GetContentMetrics(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var totalOutfits = await _unitOfWork.Repository<Outfit>().CountAsync(cancellationToken);
        var totalPosts = await _unitOfWork.Repository<FeedPost>().CountAsync(cancellationToken);
        var totalPolls = await _unitOfWork.Repository<ValidationPoll>().CountAsync(cancellationToken);
        var totalComments = 0; // Would need comments table
        var totalLikes = await _unitOfWork.Repository<Outfit>().SumAsync(o => o.LikesCount, cancellationToken) +
                         await _unitOfWork.Repository<FeedPost>().SumAsync(p => p.LikesCount, cancellationToken);

        var totalEngagement = totalLikes + totalComments;
        var totalContent = totalOutfits + totalPosts + totalPolls;
        var engagementRate = totalContent > 0 ? ((double)totalEngagement / totalContent) : 0;

        // Top content
        var topOutfits = await _unitOfWork.Repository<Outfit>()
            .OrderByDescending(o => o.LikesCount + o.CommentsCount)
            .Take(5)
            .Select(o => new ContentPerformanceData(
                o.Id,
                o.Name,
                "Outfit",
                0, // Views would need tracking
                o.LikesCount,
                o.CommentsCount,
                (o.LikesCount + o.CommentsCount) / (double)Math.Max(1, o.LikesCount + o.CommentsCount)
            ))
            .ToListAsync(cancellationToken);

        var topPosts = await _unitOfWork.Repository<FeedPost>()
            .OrderByDescending(p => p.LikesCount + p.CommentsCount)
            .Take(5)
            .Select(p => new ContentPerformanceData(
                p.Id,
                p.Title,
                "Post",
                0, // Views would need tracking
                p.LikesCount,
                p.CommentsCount,
                (p.LikesCount + p.CommentsCount) / (double)Math.Max(1, p.LikesCount + p.CommentsCount)
            ))
            .ToListAsync(cancellationToken);

        var topContent = topOutfits.Concat(topPosts).OrderByDescending(c => c.EngagementScore).Take(10).ToList();

        // Content type breakdown
        var contentTypeBreakdown = new List<ContentTypeStats>
        {
            new ContentTypeStats("Outfit", totalOutfits)
            {
                TotalEngagement = await _unitOfWork.Repository<Domain.Entities.Outfit>().SumAsync(o => o.LikesCount + o.CommentsCount, cancellationToken),
                AverageEngagement = 0
            },
            new ContentTypeStats("Post", totalPosts)
            {
                TotalEngagement = await _unitOfWork.Repository<FeedPost>().SumAsync(p => p.LikesCount + p.CommentsCount, cancellationToken),
                AverageEngagement = 0
            },
            new ContentTypeStats("Poll", totalPolls)
            {
                TotalEngagement = await _unitOfWork.Repository<ValidationPoll>().SumAsync(p => p.TotalVotes, cancellationToken),
                AverageEngagement = 0
            }
        };

        foreach (var type in contentTypeBreakdown)
        {
            type.AverageEngagement = type.Count > 0 ? ((double)type.TotalEngagement / type.Count) : 0;
        }

        return new ContentMetrics(
            totalOutfits,
            totalPosts,
            totalPolls,
            totalComments,
            totalLikes,
            engagementRate,
            topContent,
            contentTypeBreakdown
        );
    }

    private async Task<SystemPerformanceMetrics> GetSystemPerformanceMetrics(CancellationToken cancellationToken)
    {
        // In a real implementation, these would come from system monitoring
        // For now, we'll return mock data based on database performance
        return new SystemPerformanceMetrics(
            45.2, // CPU usage percentage
            512 * 1024 * 1024, // Memory usage in bytes (512MB)
            10 * 1024 * 1024 * 1024, // Disk usage in bytes (10GB)
            25, // Active connections
            125.5, // Response time in ms
            new List<PerformanceData>() // Historical data would come from monitoring system
        );
    }

    private async Task<List<TimeSeriesData>> GetTimeSeriesData(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var trends = new List<TimeSeriesData>();

        // User registrations over time
        var userRegistrations = await _unitOfWork.Repository<User>()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new TimeSeriesData(g.Key, "UserRegistrations", g.Count()))
            .ToListAsync(cancellationToken);

        trends.AddRange(userRegistrations);

        // Content creation over time
        var outfitCreations = await _unitOfWork.Repository<Outfit>()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new TimeSeriesData(g.Key, "OutfitCreations", g.Count()))
            .ToListAsync(cancellationToken);

        trends.AddRange(outfitCreations);

        return trends.OrderBy(t => t.Date).ToList();
    }

    private async Task<List<AnalyticsSummary>> GetAnalyticsSummaries(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var summaries = new List<AnalyticsSummary>();

        var currentPeriodUsers = await _unitOfWork.Repository<Domain.Entities.User>()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        var previousPeriodUsers = await _unitOfWork.Repository<Domain.Entities.User>()
            .Where(u => u.CreatedAt >= startDate.AddDays(-30) && u.CreatedAt < startDate)
            .CountAsync(cancellationToken);

        var userGrowthChange = previousPeriodUsers > 0 ? 
            ((double)(currentPeriodUsers - previousPeriodUsers) / previousPeriodUsers) * 100 : 0;

        summaries.Add(new AnalyticsSummary("Users", "New Users", currentPeriodUsers, userGrowthChange));

        var currentPeriodOutfits = await _unitOfWork.Repository<Domain.Entities.Outfit>()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        var previousPeriodOutfits = await _unitOfWork.Repository<Domain.Entities.Outfit>()
            .Where(o => o.CreatedAt >= startDate.AddDays(-30) && o.CreatedAt < startDate)
            .CountAsync(cancellationToken);

        var outfitGrowthChange = previousPeriodOutfits > 0 ? 
            ((double)(currentPeriodOutfits - previousPeriodOutfits) / previousPeriodOutfits) * 100 : 0;

        summaries.Add(new AnalyticsSummary("Content", "New Outfits", currentPeriodOutfits, outfitGrowthChange));

        return summaries;
    }
}

