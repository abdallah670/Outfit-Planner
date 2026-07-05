using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetWeeklyStyleStatsQueryHandler : IRequestHandler<GetWeeklyStyleStatsQuery, WeeklyStyleStatsDto>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IWearEventRepository _wearEventRepository;
    private readonly IUserStyleProfileRepository _styleProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public GetWeeklyStyleStatsQueryHandler(
        UserManager<Domain.Entities.User> userManager,
        IWearEventRepository wearEventRepository,
        IUserStyleProfileRepository styleProfileRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _userManager = userManager;
        _wearEventRepository = wearEventRepository;
        _styleProfileRepository = styleProfileRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<WeeklyStyleStatsDto> Handle(GetWeeklyStyleStatsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        var today = _clock.UtcNow.Date;
        var reports = new List<WeeklyReportDto>();

        var styleProfile = await _styleProfileRepository.GetByUserIdAsync(request.UserId);

        for (var weekOffset = 0; weekOffset < 4; weekOffset++)
        {
            var weekEnd = today.AddDays(-weekOffset * 7);
            var weekStart = weekEnd.AddDays(-7);

            var wearEvents = await _wearEventRepository.GetByUserIdAsync(request.UserId);
            var weekWears = wearEvents
                .Where(we => we.WornAt >= weekStart && we.WornAt < weekEnd)
                .ToList();

            var totalWears = weekWears.Count;
            var uniqueItems = weekWears.Select(we => we.ClothingItemId).Distinct().Count();
            var varietyScore = totalWears > 0 ? (double)uniqueItems / totalWears : 0.0;
            var comfortAverage = totalWears > 0 ? weekWears.Average(we => we.Rating) : 0;

            string? mostWornItemName = null;
            var mostWornCount = 0;

            if (totalWears > 0)
            {
                var topItem = weekWears
                    .GroupBy(we => we.ClothingItemId)
                    .OrderByDescending(g => g.Count())
                    .First();

                mostWornItemName = topItem.First().ClothingItem?.Name;
                mostWornCount = topItem.Count();
            }

            var trend = DetermineTrend(styleProfile, varietyScore);

            reports.Add(new WeeklyReportDto
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd.AddDays(-1),
                IsCurrentWeek = weekOffset == 0,
                MostWornItemName = mostWornItemName,
                MostWornCount = mostWornCount,
                VarietyScore = varietyScore,
                ComfortAverage = (decimal)Math.Round(comfortAverage, 1),
                TotalWears = totalWears,
                Trend = trend,
            });
        }

        return new WeeklyStyleStatsDto { WeeklyReports = reports };
    }

    private static string DetermineTrend(UserStyleProfile? styleProfile, double varietyScore)
    {
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
