using MediatR;
using OutfitPlanner.Application.DTOs.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using User = OutfitPlanner.Domain.Entities.User;

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

        var userMetrics = await GetUserEngagementMetrics(startDate, endDate, cancellationToken);
        var contentMetrics = await GetContentMetrics(startDate, endDate, cancellationToken);
        var trends = await GetTimeSeriesData(startDate, endDate, cancellationToken);
        var summaries = await GetAnalyticsSummaries(startDate, endDate, cancellationToken);

        return new DetailedAnalyticsDto(userMetrics, contentMetrics, trends, summaries);
    }

    private async Task<UserEngagementMetrics> GetUserEngagementMetrics(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var totalUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().CountAsync(cancellationToken);
        var activeUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.LastLogin >= startDate.AddDays(-30), cancellationToken);
        var newUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate, cancellationToken);
        var previousPeriodUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.CreatedAt >= startDate.AddDays(-30) && u.CreatedAt < startDate, cancellationToken);
        var userGrowthRate = previousPeriodUsers > 0 ? ((double)(newUsers - previousPeriodUsers) / previousPeriodUsers) * 100 : 0;

        var dailyActivity = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new UserActivityData(g.Key, g.Count(), g.Count(), 0))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var adminCount = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.Role == UserRole.Admin, cancellationToken);
        var plannerCount = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .CountAsync(u => u.Role == UserRole.Planner, cancellationToken);

        var demographics = new List<UserDemographics>
        {
            new("Role", "Admin", adminCount, 0.0),
            new("Role", "Planner", plannerCount, 0.0)
        };

        foreach (var demo in demographics)
        {
            demo.Percentage = totalUsers > 0 ? ((double)demo.Count / totalUsers) * 100 : 0;
        }

        return new UserEngagementMetrics(totalUsers, activeUsers, newUsers, userGrowthRate, dailyActivity, demographics);
    }

    private async Task<ContentMetrics> GetContentMetrics(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var totalOutfits = await _unitOfWork.Repository<Outfit>().CountAsync(cancellationToken);
        var totalPosts = await _unitOfWork.Repository<FeedPost>().CountAsync(cancellationToken);
        var totalPolls = await _unitOfWork.Repository<ValidationPoll>().CountAsync(cancellationToken);
        var totalComments = 0;
        var totalLikes = await _unitOfWork.Repository<Outfit>().GetQueryable().SumAsync(o => o.LikesCount + o.CommentsCount, cancellationToken) +
                         await _unitOfWork.Repository<FeedPost>().GetQueryable().SumAsync(p => p.LikesCount + p.CommentsCount, cancellationToken);

        var totalEngagement = (int)totalLikes + totalComments;
        var totalContent = totalOutfits + totalPosts + totalPolls;
        var engagementRate = totalContent > 0 ? ((double)totalEngagement / totalContent) : 0;

        var topOutfits = await _unitOfWork.Repository<Outfit>().GetQueryable()
            .OrderByDescending(o => o.LikesCount + o.CommentsCount)
            .Take(5)
            .Select(o => new ContentPerformanceData(
                o.Id,
                o.Name,
                "Outfit",
                0,
                o.LikesCount,
                o.CommentsCount,
                (o.LikesCount + o.CommentsCount) / (double)Math.Max(1, o.LikesCount + o.CommentsCount)
            ))
            .ToListAsync(cancellationToken);

        var topPosts = await _unitOfWork.Repository<FeedPost>().GetQueryable()
            .OrderByDescending(p => p.LikesCount + p.CommentsCount)
            .Take(5)
            .Select(p => new ContentPerformanceData(
                p.Id,
                p.Caption ?? "",
                "Post",
                0,
                p.LikesCount,
                p.CommentsCount,
                (p.LikesCount + p.CommentsCount) / (double)Math.Max(1, p.LikesCount + p.CommentsCount)
            ))
            .ToListAsync(cancellationToken);

        var topContent = topOutfits.Concat(topPosts).OrderByDescending(c => c.EngagementScore).Take(10).ToList();

        var outfitEngagement = await _unitOfWork.Repository<Outfit>().GetQueryable().SumAsync(o => o.LikesCount + o.CommentsCount, cancellationToken);
        var contentTypeBreakdown = new List<ContentTypeStats>
        {
            new ContentTypeStats("Outfit", totalOutfits, (int)outfitEngagement, 0),
            new ContentTypeStats("Post", totalPosts, 0, 0),
            new ContentTypeStats("Poll", totalPolls, (int)await _unitOfWork.Repository<ValidationPoll>().GetQueryable().SumAsync(p => p.TotalVotes, cancellationToken), 0)
        };

        return new ContentMetrics(totalOutfits, totalPosts, totalPolls, totalComments, totalLikes, engagementRate, topContent, contentTypeBreakdown);
    }


    private async Task<List<TimeSeriesData>> GetTimeSeriesData(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var trends = new List<TimeSeriesData>();

        var userRegistrations = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new TimeSeriesData(g.Key, "UserRegistrations", g.Count()))
            .ToListAsync(cancellationToken);

        trends.AddRange(userRegistrations);

        var outfitCreations = await _unitOfWork.Repository<Outfit>().GetQueryable()
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

        var currentPeriodUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        var previousPeriodUsers = await _unitOfWork.Repository<OutfitPlanner.Domain.Entities.User>().GetQueryable()
            .Where(u => u.CreatedAt >= startDate.AddDays(-30) && u.CreatedAt < startDate)
            .CountAsync(cancellationToken);

        var userGrowthChange = previousPeriodUsers > 0 ? ((double)(currentPeriodUsers - previousPeriodUsers) / previousPeriodUsers) * 100 : 0;

        summaries.Add(new AnalyticsSummary("Users", "TotalUsers", currentPeriodUsers, userGrowthChange));

        var currentPeriodOutfits = await _unitOfWork.Repository<Outfit>().GetQueryable()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        var previousPeriodOutfits = await _unitOfWork.Repository<Outfit>().GetQueryable()
            .Where(o => o.CreatedAt >= startDate.AddDays(-30) && o.CreatedAt < startDate)
            .CountAsync(cancellationToken);

        var outfitGrowthChange = previousPeriodOutfits > 0 ? ((double)(currentPeriodOutfits - previousPeriodOutfits) / previousPeriodOutfits) * 100 : 0;

        summaries.Add(new AnalyticsSummary("Outfits", "TotalOutfits", currentPeriodOutfits, outfitGrowthChange));

        return summaries;
    }
}
