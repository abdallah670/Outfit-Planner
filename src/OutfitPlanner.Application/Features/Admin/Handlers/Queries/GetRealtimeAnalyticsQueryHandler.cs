using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using OutfitPlanner.Domain.Entities;
using UserEngagementMetrics = OutfitPlanner.Application.DTOs.Admin.UserEngagementMetrics;
using ContentMetrics = OutfitPlanner.Application.DTOs.Admin.ContentMetrics;
using SystemPerformanceMetrics = OutfitPlanner.Application.DTOs.Admin.SystemPerformanceMetrics;
using OutfitPlanner.Domain.Entities;
using ValidationPoll = OutfitPlanner.Domain.Entities.ValidationPoll;
using FeedPost = OutfitPlanner.Domain.Entities.FeedPost;
using Outfit = OutfitPlanner.Domain.Entities.Outfit;
using Poll = OutfitPlanner.Domain.Entities.ValidationPoll;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;


public class GetRealtimeAnalyticsQueryHandler : IRequestHandler<GetRealtimeAnalyticsQuery, RealtimeAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRealtimeAnalyticsQueryHandler> _logger;

    public GetRealtimeAnalyticsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetRealtimeAnalyticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RealtimeAnalyticsDto> Handle(GetRealtimeAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);

        // Real-time user metrics
        var userMetrics = await GetRealtimeUserMetrics(last24Hours, now, cancellationToken);
        
        // Real-time content metrics
        var contentStats = await GetRealtimeContentMetrics(last24Hours, now, cancellationToken);
        
        // Real-time system performance
        var systemStats = await GetRealtimeSystemMetrics(cancellationToken);

        return new RealtimeAnalyticsDto(
            userMetrics,
            contentStats,
            systemStats,
            now
        );
    }

    private async Task<UserEngagementMetrics> GetRealtimeUserMetrics(DateTime startDate, DateTime now, CancellationToken cancellationToken)
    {
        var totalUsers = await _unitOfWork.Repository<User>().CountAsync(cancellationToken);
        
        var activeUsers = await _unitOfWork.Repository<User>()
            .Where(u => _unitOfWork.Repository<Outfit>().Any(o => o.CreatedById == u.Id && o.CreatedAt >= startDate) ||
                       _unitOfWork.Repository<FeedPost>().Any(p => p.CreatedById == u.Id && p.CreatedAt >= startDate))
            .CountAsync(cancellationToken);

        var newUsers = await _unitOfWork.Repository<User>()
            .Where(u => u.CreatedAt >= startDate)
            .CountAsync(cancellationToken);

        var previousDayUsers = await _unitOfWork.Repository<User>()
            .Where(u => u.CreatedAt >= startDate.AddDays(-1) && u.CreatedAt < startDate)
            .CountAsync(cancellationToken);

        var userGrowthRate = previousDayUsers > 0 ? 
            ((double)(newUsers - previousDayUsers) / previousDayUsers) * 100 : 0;

        // Real-time daily activity
        var dailyActivity = new List<UserActivityData>
        {
            new UserActivityData(now.Date, activeUsers, newUsers, activeUsers - newUsers)
        };

        // Real-time demographics
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

    private async Task<ContentMetrics> GetRealtimeContentMetrics(DateTime startDate, DateTime now, CancellationToken cancellationToken)
    {
        var totalOutfits = await _unitOfWork.Repository<Outfit>().CountAsync(cancellationToken);
        var totalPosts = await _unitOfWork.Repository<FeedPost>().CountAsync(cancellationToken);
        var totalUsers = await _unitOfWork.Repository<User>().CountAsync(cancellationToken);
        var activeUsers = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddHours(-24), cancellationToken);
        var newUsers = await _unitOfWork.Repository<User>()
            .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30), cancellationToken);

        var newOutfits = await _unitOfWork.Repository<Outfit>()
            .CountAsync(o => o.CreatedAt >= startDate, cancellationToken);

        var newPosts = await _unitOfWork.Repository<FeedPost>()
            .CountAsync(p => p.CreatedAt >= startDate, cancellationToken);

        var newPolls = await _unitOfWork.Repository<Poll>()
            .CountAsync(p => p.CreatedAt >= startDate, cancellationToken);

        var totalLikes = await _unitOfWork.Repository<Outfit>().SumAsync(o => o.LikesCount, cancellationToken) +
                         await _unitOfWork.Repository<FeedPost>().SumAsync(p => p.LikesCount, cancellationToken);

        var totalComments = 0; // Would need comments table
        var totalContent = totalOutfits + totalPosts + totalPolls;
        var engagementRate = totalContent > 0 ? ((double)(totalLikes + totalComments) / totalContent) : 0;

        // Real-time top content
        var topOutfits = await _unitOfWork.Repository<Outfit>()
            .GetQueryable(o => o.CreatedAt >= startDate)
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
            .Where(p => p.CreatedAt >= startDate)
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

        // Real-time content type breakdown
        var contentTypeBreakdown = new List<ContentTypeStats>
        {
            new("Outfit", totalOutfits, await _unitOfWork.Repository<Outfit>().SumAsync(o => o.LikesCount + o.CommentsCount, cancellationToken), 0),
            new("Post", totalPosts, await _unitOfWork.Repository<FeedPost>().SumAsync(p => p.LikesCount + p.CommentsCount, cancellationToken), 0),
            new("Poll", totalPolls, await _unitOfWork.Repository<ValidationPoll>().SumAsync(p => p.TotalVotes, cancellationToken), 0)
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

    private async Task<SystemPerformanceMetrics> GetRealtimeSystemMetrics(CancellationToken cancellationToken)
    {
        // In a real implementation, these would come from system monitoring services
        // For now, we'll simulate real-time data
        return new SystemPerformanceMetrics(
            new Random().NextDouble() * 100, // CPU usage percentage
            (long)(new Random().NextDouble() * 1024 * 1024 * 1024), // Memory usage in bytes
            (long)(new Random().NextDouble() * 100 * 1024 * 1024 * 1024), // Disk usage in bytes
            new Random().Next(10, 100), // Active connections
            new Random().NextDouble() * 200, // Response time in ms
            new List<PerformanceData>() // Historical data would come from monitoring system
        );
    }
}
