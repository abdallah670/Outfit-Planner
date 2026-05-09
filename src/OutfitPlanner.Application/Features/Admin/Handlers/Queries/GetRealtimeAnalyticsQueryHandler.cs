using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using User = OutfitPlanner.Domain.Entities.User;
using UserEngagementMetrics = OutfitPlanner.Application.DTOs.Admin.UserEngagementMetrics;
using ContentMetrics = OutfitPlanner.Application.DTOs.Admin.ContentMetrics;
using UserActivityData = OutfitPlanner.Application.DTOs.Admin.UserActivityData;
using UserDemographics = OutfitPlanner.Application.DTOs.Admin.UserDemographics;
using ContentPerformanceData = OutfitPlanner.Application.DTOs.Admin.ContentPerformanceData;
using ContentTypeStats = OutfitPlanner.Application.DTOs.Admin.ContentTypeStats;
using OutfitPlanner.Domain.Enums;
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
        
  

        return new RealtimeAnalyticsDto(
            userMetrics,
            contentStats,
            now
        );
    }

    private async Task<UserEngagementMetrics> GetRealtimeUserMetrics(DateTime startDate, DateTime now, CancellationToken cancellationToken)
    {
        var totalUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().CountAsync(cancellationToken);
        
        var activeUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .Where(u => _unitOfWork.Repository<Outfit>().GetQueryable().Any(o => o.UserId == u.Id && o.CreatedAt >= startDate) ||
                       _unitOfWork.Repository<FeedPost>().GetQueryable().Any(p => p.UserId == u.Id && p.CreatedAt >= startDate))
            .CountAsync(cancellationToken);

        var newUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .Where(u => u.CreatedAt >= startDate)
            .CountAsync(cancellationToken);

        var previousDayUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
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
            new("Role", "Admin", await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable().CountAsync(u => u.Role == UserRole.Admin, cancellationToken), 0),
            new("Role", "Planner", await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable().CountAsync(u => u.Role == UserRole.Planner, cancellationToken), 0)
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
        var totalUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().CountAsync(cancellationToken);
        
        var activeUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.LastLogin >= DateTime.UtcNow.AddHours(-24), cancellationToken);
        var newUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30), cancellationToken);

        var newOutfits = await _unitOfWork.Repository<Outfit>().GetQueryable()
            .CountAsync(o => o.CreatedAt >= startDate, cancellationToken);

        var newPosts = await _unitOfWork.Repository<FeedPost>().GetQueryable()
            .CountAsync(p => p.CreatedAt >= startDate, cancellationToken);

        var totalPolls = await _unitOfWork.Repository<ValidationPoll>().CountAsync(cancellationToken);
        var newPolls = await _unitOfWork.Repository<ValidationPoll>().GetQueryable()
            .CountAsync(p => p.CreatedAt >= startDate, cancellationToken);

        var totalLikes = await _unitOfWork.Repository<Outfit>().GetQueryable().SumAsync(o => o.LikesCount, cancellationToken) +
                         await _unitOfWork.Repository<FeedPost>().GetQueryable().SumAsync(p => p.LikesCount, cancellationToken);

        var totalComments = 0; // Would need comments table
        var totalContent = totalOutfits + totalPosts + totalPolls;
        var engagementRate = totalContent > 0 ? ((double)(totalLikes + totalComments) / totalContent) : 0;

        // Real-time top content
        var topOutfits = await _unitOfWork.Repository<Outfit>()
            .GetQueryable()
            .Where(o => o.CreatedAt >= startDate)
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
            .GetQueryable()
            .Where(p => p.CreatedAt >= startDate)
            .OrderByDescending(p => p.LikesCount + p.CommentsCount)
            .Take(5)
            .Select(p => new ContentPerformanceData(
                p.Id,
                p.Caption,
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
            new("Outfit", totalOutfits, await _unitOfWork.Repository<Outfit>().GetQueryable().SumAsync(o => o.LikesCount + o.CommentsCount, cancellationToken), 0),
            new("Post", totalPosts, await _unitOfWork.Repository<FeedPost>().GetQueryable().SumAsync(p => p.LikesCount + p.CommentsCount, cancellationToken), 0),
            new("Poll", totalPolls, await _unitOfWork.Repository<ValidationPoll>().GetQueryable().SumAsync(p => p.TotalVotes, cancellationToken), 0)
        };

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

  
}
