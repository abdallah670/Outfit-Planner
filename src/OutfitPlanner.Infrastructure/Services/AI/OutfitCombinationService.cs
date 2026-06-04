using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class OutfitCombinationService : IOutfitCombinationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IColorHarmonyService _colorHarmonyService;
    private readonly IStyleCompatibilityService _styleCompatibilityService;

    public OutfitCombinationService(
        IUnitOfWork unitOfWork,
        IColorHarmonyService colorHarmonyService,
        IStyleCompatibilityService styleCompatibilityService)
    {
        _unitOfWork = unitOfWork;
        _colorHarmonyService = colorHarmonyService;
        _styleCompatibilityService = styleCompatibilityService;
    }

    public async Task<OutfitCombinationResult> GenerateCombinationsAsync(
        string userId,
        string? occasion,
        string? weatherCondition,
        int maxResults = 3,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Domain.Entities.ClothingItem>()
            .GetQueryable(c => c.UserId == userId && !c.IsDeleted);

        var items = await query.ToListAsync(cancellationToken);

        if (!items.Any())
        {
            return new OutfitCombinationResult
            {
                Combinations = new List<OutfitCombinationItem>(),
                Error = "No clothing items found in your wardrobe."
            };
        }

        var tops = items.Where(i => IsTop(i.Type.ToString() ?? i.Category ?? "")).ToList();
        var bottoms = items.Where(i => IsBottom(i.Type.ToString() ?? i.Category ?? "")).ToList();
        var footwear = items.Where(i => IsFootwear(i.Type.ToString() ?? i.Category ?? "")).ToList();
        var outerwear = items.Where(i => IsOuterwear(i.Type.ToString() ?? i.Category ?? "")).ToList();

        var combinations = new List<(double Score, List<ClothingItemRef> Items)>();

        foreach (var top in tops)
        {
            foreach (var bottom in bottoms)
            {
                foreach (var shoe in footwear.DefaultIfEmpty())
                {
                    var comboItems = new List<ClothingItemRef> { ToRef(top), ToRef(bottom) };
                    if (shoe != null) comboItems.Add(ToRef(shoe));

                    if (outerwear.Any())
                    {
                        var bestOuter = outerwear
                            .OrderByDescending(o => ScoreItemPair(top, o))
                            .First();
                        comboItems.Add(ToRef(bestOuter));
                    }

                    var colors = comboItems.Select(i => i.HexColor).ToList();
                    var harmony = await _colorHarmonyService.CalculateHarmonyAsync(colors, cancellationToken);

                    var score = 60.0 + (harmony.Score * 0.25);
                    if (occasion != null) score += 5;
                    if (weatherCondition != null) score += 5;

                    combinations.Add((Math.Round(Math.Min(score, 100), 1), comboItems));
                }
            }
        }

        var ranked = combinations
            .OrderByDescending(c => c.Score)
            .Take(maxResults)
            .Select((c, i) => new OutfitCombinationItem
            {
                Rank = i + 1,
                Items = c.Items,
                TotalScore = c.Score,
                ScoreBreakdown = new Dictionary<string, double>
                {
                    ["Total"] = c.Score,
                    ["Color Harmony"] = Math.Round(c.Score * 0.4, 1),
                    ["Completeness"] = Math.Round(c.Score * 0.3, 1),
                    ["Occasion Fit"] = Math.Round(c.Score * 0.3, 1)
                }
            }).ToList();

        return new OutfitCombinationResult
        {
            Combinations = ranked,
            Error = ranked.Any() ? null : "Could not generate any valid outfit combinations."
        };
    }

    private static ClothingItemRef ToRef(Domain.Entities.ClothingItem item)
    {
        return new ClothingItemRef
        {
            Id = item.Id.ToString(),
            Name = item.Name ?? "Unknown",
            Type = item.Type.ToString(),
            ImageUrl = item.ImageUrl ?? string.Empty,
            HexColor = item.PrimaryColor ?? "#636E72"
        };
    }

    private static double ScoreItemPair(Domain.Entities.ClothingItem a, Domain.Entities.ClothingItem b)
    {
        var score = 70.0;
        if (a.PrimaryColor != null && b.PrimaryColor != null && a.PrimaryColor != b.PrimaryColor)
            score += 10;
        return Math.Min(score, 100);
    }

    private static bool IsTop(string type) =>
        type.Contains("Top", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("shirt", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("blouse", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("sweater", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("hoodie", StringComparison.OrdinalIgnoreCase);

    private static bool IsBottom(string type) =>
        type.Contains("Bottom", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("pant", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("jean", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("short", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("skirt", StringComparison.OrdinalIgnoreCase);

    private static bool IsFootwear(string type) =>
        type.Contains("Footwear", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("shoe", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("sneaker", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("boot", StringComparison.OrdinalIgnoreCase);

    private static bool IsOuterwear(string type) =>
        type.Contains("Outerwear", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("jacket", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("coat", StringComparison.OrdinalIgnoreCase) ||
        type.Contains("blazer", StringComparison.OrdinalIgnoreCase);
}