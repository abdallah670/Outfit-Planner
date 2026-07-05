using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Infrastructure.Services;

public class WeeklyReportService
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WeeklyReportService> _logger;
    private readonly IClock _clock;

    public WeeklyReportService(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ILogger<WeeklyReportService> logger,
        IClock clock)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _clock = clock;
    }

    public async Task GenerateWeeklyReportAsync()
    {
        try
        {
            _logger.LogInformation("Hangfire: Starting weekly style report job at {Time}", _clock.UtcNow);

            var today = _clock.UtcNow.Date;
            var weekEnd = today;
            var weekStart = weekEnd.AddDays(-7);

            var users = await _unitOfWork.Users.GetAllAsync();
            var totalUsers = users.Count();

            if (totalUsers == 0)
            {
                _logger.LogInformation("Hangfire: No users found for weekly report");
                return;
            }

            if (totalUsers > 1000)
            {
                _logger.LogWarning("Hangfire: Large user base ({UserCount}) detected for weekly report. Consider batching.", totalUsers);
            }

            var wearEvents = _unitOfWork.WearEvents.GetQueryable()
                .Where(we => we.WornAt >= weekStart && we.WornAt < weekEnd)
                .Include(we => we.ClothingItem)
                .ToList();

            if (!wearEvents.Any())
            {
                _logger.LogInformation("Hangfire: No wear events found for last week");
                return;
            }

            var eventsByUser = wearEvents.GroupBy(we => we.UserId);
            var processedCount = 0;

            foreach (var userGroup in eventsByUser)
            {
                try
                {
                    var userId = userGroup.Key;
                    var userWears = userGroup.Where(we => we.ClothingItemId.HasValue).ToList();

                    if (userWears.Count == 0)
                        continue;

                    var topItem = userWears
                        .GroupBy(we => we.ClothingItemId!.Value)
                        .OrderByDescending(g => g.Count())
                        .First();

                    var topItemId = topItem.Key;
                    var mostWornCount = topItem.Count();
                    var itemName = topItem.First().ClothingItem?.Name ?? "your favorite item";

                    var uniqueItems = userWears.Select(we => we.ClothingItemId!.Value).Distinct().Count();
                    var totalWears = userWears.Count;
                    var varietyScore = (double)uniqueItems / totalWears;

                    var comfortAvg = userWears.Average(we => we.Rating);

                    string trend = DetermineTrend(userId, varietyScore);

                    var message = $"You wore {itemName} {mostWornCount} times last week. Your style: {trend}.";

                    await _mediator.Send(new CreateNotificationCommand
                    {
                        UserId = userId,
                        Request = new CreateNotificationDto
                        {
                            Type = NotificationType.System,
                            Title = "Weekly Style Report Ready",
                            Message = message,
                            ActionUrl = "/profile/stats"
                        }
                    });

                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hangfire: Failed to generate weekly report for user {UserId}", userGroup.Key);
                }
            }

            _logger.LogInformation("Hangfire: Weekly style report job completed. Processed {ProcessedCount} users", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hangfire: Error in weekly style report job");
            throw;
        }
    }

    private string DetermineTrend(string userId, double varietyScore)
    {
        var styleProfile = _unitOfWork.UserStyleProfiles.GetQueryable()
            .FirstOrDefault(sp => sp.UserId == userId);

        if (styleProfile != null)
        {
            return styleProfile.Style.ToString();
        }

        if (varietyScore > 0.7)
            return "Versatile";
        if (varietyScore < 0.3)
            return "Focused";
        return "Mixed";
    }
}
