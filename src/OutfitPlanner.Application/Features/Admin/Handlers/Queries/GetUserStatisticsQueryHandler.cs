using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, UserStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserStatisticsQueryHandler> _logger;

    public GetUserStatisticsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserStatisticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserStatisticsDto> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting statistics for user {UserId}", request.UserId);

        // Parse the UserId string to Guid
        if (!Guid.TryParse(request.UserId, out var userIdGuid))
        {
            return new UserStatisticsDto(0, 0, 0, 0, null, new List<MonthlyActivityDto>());
        }

        // Get user's content for statistics
        var userOutfits = await _unitOfWork.Repository<Outfit>().GetQueryable()
            .Where(o => o.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var userPosts = await _unitOfWork.Repository<FeedPost>().GetQueryable()
            .Where(p => p.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var userComments = await _unitOfWork.Repository<PostComment>().GetQueryable()
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var userReactions = await _unitOfWork.Repository<PostReaction>().GetQueryable()
            .Where(r => r.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        // Calculate last active date
        var lastActiveDate = userOutfits.Select(o => o.CreatedAt)
            .Concat(userPosts.Select(p => p.CreatedAt))
            .Concat(userComments.Select(c => c.CreatedAt))
            .Concat(userReactions.Select(r => r.CreatedAt))
            .OrderByDescending(t => t)
            .Cast<DateTimeOffset?>()
            .FirstOrDefault();
            
        DateTime? lastActive = lastActiveDate?.DateTime;

        // Calculate monthly activity for the last 12 months
        var monthlyActivity = new List<MonthlyActivityDto>();
        var currentDate = DateTime.UtcNow;
        
        for (int i = 0; i < 12; i++)
        {
            var monthDate = currentDate.AddMonths(-i);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var outfitsInMonth = userOutfits.Count(o => o.CreatedAt.DateTime >= monthStart && o.CreatedAt.DateTime <= monthEnd);
            var postsInMonth = userPosts.Count(p => p.CreatedAt.DateTime >= monthStart && p.CreatedAt.DateTime <= monthEnd);
            var commentsInMonth = userComments.Count(c => c.CreatedAt.DateTime >= monthStart && c.CreatedAt.DateTime <= monthEnd);

            monthlyActivity.Add(new MonthlyActivityDto(
                monthDate.Month,
                monthDate.Year,
                outfitsInMonth,
                postsInMonth,
                commentsInMonth
            ));
        }

        return new UserStatisticsDto(
            userOutfits.Count,
            userPosts.Count,
            userComments.Count,
            userReactions.Count,
            lastActive,
            monthlyActivity.OrderByDescending(m => m.Year).ThenByDescending(m => m.Month).ToList()
        );
    }
}
