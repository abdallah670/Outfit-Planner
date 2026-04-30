using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries;

public class GetWardrobeHealthRequestHandler : IRequestHandler<GetWardrobeHealthRequest, WardrobeHealthDto>
{
    private readonly ILogger<GetWardrobeHealthRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public GetWardrobeHealthRequestHandler(ILogger<GetWardrobeHealthRequestHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<WardrobeHealthDto> Handle(GetWardrobeHealthRequest request, CancellationToken cancellationToken)
    {
        var clothingItems = await _unitOfWork.ClothingItems.GetByUserIdAsync(request.UserId);

        var items = clothingItems.ToList();
        var totalItems = items.Count;

        if (totalItems == 0)
        {
            return new WardrobeHealthDto
            {
                HealthPercentage = 0,
                TotalItems = 0,
                WornItems = 0,
                UnwornItems = 0,
                TotalWears = 0,
                AverageWearsPerItem = 0,
                MostWornItemName = null,
                MostWornItemCount = 0
            };
        }

        var wornItems = items.Where(i => i.WearCount > 0).ToList();
        var wornCount = wornItems.Count;
        var totalWears = items.Sum(i => i.WearCount);
        var mostWornItem = items.OrderByDescending(i => i.WearCount).FirstOrDefault();

        var healthPercentage = (int)Math.Round((double)wornCount / totalItems * 100);

        var result = new WardrobeHealthDto
        {
            HealthPercentage = healthPercentage,
            TotalItems = totalItems,
            WornItems = wornCount,
            UnwornItems = totalItems - wornCount,
            TotalWears = totalWears,
            AverageWearsPerItem = Math.Round((double)totalWears / totalItems, 2),
            MostWornItemName = mostWornItem?.Name,
            MostWornItemCount = mostWornItem?.WearCount ?? 0
        };

        _logger.LogInformation(
            "Wardrobe health calculated for user {UserId}: {HealthPercentage}% ({WornItems}/{TotalItems} items worn)",
            request.UserId, healthPercentage, wornCount, totalItems);

        return result;
    }
}
