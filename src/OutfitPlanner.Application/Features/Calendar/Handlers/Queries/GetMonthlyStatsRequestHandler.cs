using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Queries;

public class GetMonthlyStatsRequestHandler : IRequestHandler<GetMonthlyStatsRequest, MonthlyStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMonthlyStatsRequestHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MonthlyStatsDto> Handle(GetMonthlyStatsRequest request, CancellationToken cancellationToken)
    {
        var startDate = new DateTimeOffset(new DateTime(request.Year, request.Month, 1));
        var endDate = startDate.AddMonths(1);
        var now = DateTimeOffset.UtcNow;

        var wearEvents = await _unitOfWork.WearEvents
            .FindAsync(we => we.UserId == request.UserId && we.WornAt >= startDate && we.WornAt < endDate);

        var events = wearEvents.ToList();
        var wornEvents = events.Where(we => we.WornAt <= now).ToList();
        var scheduledEvents = events.Where(we => we.WornAt > now).ToList();

        // Calculate favorite outfit
        var favoriteGroup = wornEvents
            .Where(we => we.OutfitId.HasValue)
            .GroupBy(we => we.OutfitId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        string? favoriteOutfitName = null;
        if (favoriteGroup?.Key != null)
        {
            var outfit = await _unitOfWork.Outfits.GetByIdAsync(favoriteGroup.Key.Value);
            favoriteOutfitName = outfit?.Name;
        }

        var stats = new MonthlyStatsDto
        {
            Year = request.Year,
            Month = request.Month,
            TotalWorn = wornEvents.Count,
            TotalScheduled = events.Count, // Total events scheduled for the month (past + future)
            UniqueOutfitsWorn = wornEvents.Where(we => we.OutfitId.HasValue).Select(we => we.OutfitId).Distinct().Count(),
            FavoriteOutfitId = favoriteGroup?.Key,
            FavoriteOutfitName = favoriteOutfitName,
            FavoriteWearCount = favoriteGroup?.Count() ?? 0,
            OutfitsByOccasion = new Dictionary<string, int>()
        };

        return stats;
    }
}
