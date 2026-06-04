using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Infrastructure.Services.AI;

public class StyleCompatibilityService : IStyleCompatibilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IColorHarmonyService _colorHarmonyService;

    public StyleCompatibilityService(IUnitOfWork unitOfWork, IColorHarmonyService colorHarmonyService)
    {
        _unitOfWork = unitOfWork;
        _colorHarmonyService = colorHarmonyService;
    }

    public async Task<StyleScoreResult> CalculateScoreAsync(
        IEnumerable<string> clothingItemIds,
        string occasion,
        string? weatherCondition,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var breakdown = new Dictionary<string, double>();
        var suggestions = new List<string>();

        // Fetch all clothing items in a single query
        var itemGuids = clothingItemIds
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        var items = await _unitOfWork.Repository<ClothingItem>()
            .GetQueryable(c => itemGuids.Contains(c.Id) && !c.IsDeleted).Select(c => new { Id = c.Id, PrimaryColor = c.PrimaryColor,
             Type = c.Type, WearCount = c.WearCount })
            .ToListAsync(cancellationToken);

        if (!items.Any())
        {
            return new StyleScoreResult
            {
                TotalScore = 50,
                Breakdown = new Dictionary<string, double> { { "Error", 0 } },
                Summary = "No valid clothing items found to evaluate.",
                Suggestions = new List<string> { "Add items to your wardrobe first." }
            };
        }

        // Color harmony score (30% weight) — use actual item colors
        var hexColors = items.Select(i => i.PrimaryColor).Where(c => !string.IsNullOrEmpty(c)).ToList();
        double colorScore = 70;
        if (hexColors.Count >= 2)
        {
            var harmony = await _colorHarmonyService.CalculateHarmonyAsync(hexColors, cancellationToken);
            colorScore = harmony.Score;
        }
        breakdown["Color Harmony"] = colorScore;

        // Type completeness score (25% weight) — check if outfit has top + bottom + footwear
        var types = items.Select(i => i.Type.ToString()).ToList();
        var hasTop = types.Any(t => t.Contains("Top") || t.Contains("Dress"));
        var hasBottom = types.Any(t => t.Contains("Bottom") || t.Contains("Dress"));
        var hasFootwear = types.Any(t => t.Contains("Footwear"));
        var completenessScore = (hasTop ? 30 : 0) + (hasBottom ? 30 : 0) + (hasFootwear ? 40 : 0);
        breakdown["Completeness"] = completenessScore;
        if (!hasTop) suggestions.Add("Add a top or dress to complete the outfit.");
        if (!hasBottom) suggestions.Add("Add a bottom piece.");
        if (!hasFootwear) suggestions.Add("Add footwear.");

        // Wear pattern score (15% weight) — prefer less-worn items
        var wearScores = items.Select(i => Math.Max(0, 100 - (i.WearCount * 5)));
        var wearScore = wearScores.Any() ? wearScores.Average() : 70;
        breakdown["Wear Balance"] = Math.Round(wearScore, 1);

        // Occasion fit score (20% weight)
        var occasionScore = ScoreOccasionFit(occasion);
        breakdown["Occasion Fit"] = occasionScore;
        if (occasionScore < 60)
            suggestions.Add($"Consider different items to better suit '{occasion}'.");

        // Weather fit score (10% weight)
        var weatherScore = ScoreWeatherFit(weatherCondition);
        breakdown["Weather Fit"] = weatherScore;
        if (weatherScore < 60 && !string.IsNullOrEmpty(weatherCondition))
            suggestions.Add($"The weather ({weatherCondition}) may not be ideal.");

        // Total: weighted average
        var totalScore = (colorScore * 0.30) + (completenessScore * 0.25) +
                         (wearScore * 0.15) + (occasionScore * 0.20) + (weatherScore * 0.10);
        totalScore = Math.Round(Math.Min(totalScore, 100), 1);

        var summary = totalScore switch
        {
            >= 85 => "This outfit looks great! Excellent combination of colors and pieces.",
            >= 70 => "This outfit works well. A few minor tweaks could make it perfect.",
            >= 55 => "This outfit is decent but needs some adjustments.",
            _ => "This outfit needs rethinking. Consider different item combinations."
        };

        if (!suggestions.Any())
            suggestions.Add("Your outfit looks well-balanced!");

        return new StyleScoreResult
        {
            TotalScore = totalScore,
            Breakdown = breakdown,
            Summary = summary,
            Suggestions = suggestions
        };
    }

    private static double ScoreOccasionFit(string occasion)
    {
        return occasion?.ToLowerInvariant() switch
        {
            "casual" => 85,
            "formal" => 75,
            "business" => 80,
            "beach" => 80,
            "party" => 75,
            "date" => 78,
            "interview" => 72,
            "workout" => 85,
            "wedding" => 70,
            "outdoor" => 80,
            _ => 75
        };
    }

    private static double ScoreWeatherFit(string? weather)
    {
        return weather?.ToLowerInvariant() switch
        {
            "sunny" => 85,
            "clear" => 85,
            "cloudy" => 80,
            "rain" => 60,
            "rainy" => 60,
            "storm" => 50,
            "snow" => 55,
            "cold" => 65,
            "hot" => 70,
            "warm" => 82,
            "windy" => 75,
            _ => 75
        };
    }
}