using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class WardrobeContextBuilder : IWardrobeContextBuilder
{
    private readonly IUnitOfWork _unitOfWork;

    public WardrobeContextBuilder(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WardrobeContext> BuildContextAsync(
        string userId,
        IntentResult intent,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Domain.Entities.ClothingItem>()
            .GetQueryable(c => c.UserId == userId && !c.IsDeleted);

        var items = await query
            .Take(20)
            .ToListAsync(cancellationToken);

        var availableItems = items.Select(c => new WardrobeItemContext
        {
            Id = c.Id.ToString(),
            Name = c.Name ?? "Unknown",
            Type = c.Type.ToString(),
            Category = c.Category ?? "Other",
            PrimaryColor = c.PrimaryColor ?? "#636E72",
            ImageUrl = c.ImageUrl ?? string.Empty,
            WearCount = c.WearCount
        }).ToList();

        return new WardrobeContext
        {
            UserId = userId,
            AvailableItems = availableItems,
            UserStyleProfile = "casual",
            RecentlyWornItemIds = items
                .Where(c => c.LastWorn.HasValue)
                .OrderByDescending(c => c.LastWorn)
                .Take(5)
                .Select(c => c.Id.ToString())
                .ToList()
        };
    }
}