using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetDashboardAnalyticsQueryHandler : IRequestHandler<GetDashboardAnalyticsQuery, AnalyticsDashboardDto>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDashboardAnalyticsQueryHandler> _logger;

    public GetDashboardAnalyticsQueryHandler(UserManager<Domain.Entities.User> userManager, IUnitOfWork unitOfWork, ILogger<GetDashboardAnalyticsQueryHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AnalyticsDashboardDto> Handle(GetDashboardAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var start = request.StartDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = request.EndDate ?? DateTimeOffset.UtcNow;
        
        var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
        var newUsersToday = await _userManager.Users
            .CountAsync(u => u.CreatedAt >= DateTimeOffset.UtcNow.Date, cancellationToken);
        var newUsersThisWeek = await _userManager.Users
            .CountAsync(u => u.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-7), cancellationToken);
        var activeUsers = await GetActiveUserCount(start, end, cancellationToken);
        
        var totalOutfits = await _unitOfWork.Repository<Outfit>().CountAsync();
        var totalPosts = await _unitOfWork.Repository<FeedPost>().CountAsync();
        var totalPolls = await _unitOfWork.Repository<Poll>().CountAsync();
        
        var pendingReports = await _unitOfWork.Repository<ContentReport>()
            .CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);
        var resolvedReports = await _unitOfWork.Repository<ContentReport>()
            .CountAsync(r => r.Status == ReportStatus.Resolved, cancellationToken);
        
        var lockedAccounts = await _userManager.Users
            .CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow, cancellationToken);
        var bannedUsers = await _userManager.Users
            .CountAsync(u => u.Claims.Any(c => c.Type == "Banned" && c.Value == "true"), cancellationToken);
        
        return new AnalyticsDashboardDto(
            totalUsers,
            newUsersToday,
            activeUsers,
            totalOutfits,
            totalPosts,
            totalPolls,
            pendingReports,
            resolvedReports,
            lockedAccounts,
            bannedUsers
        );
    }
    
    private async Task<int> GetActiveUserCount(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
    {
        // This is a simplified version - in production you'd track actual logins
        return await _userManager.Users
            .CountAsync(u => u.LastLoginAt >= start && u.LastLoginAt <= end, cancellationToken);
    }
}
