namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// DTO for monthly calendar statistics
/// </summary>
public class MonthlyStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalWorn { get; set; }
    public int TotalScheduled { get; set; }
    public int UniqueOutfitsWorn { get; set; }
    public Guid? FavoriteOutfitId { get; set; }
    public string? FavoriteOutfitName { get; set; }
    public int FavoriteWearCount { get; set; }
    public Dictionary<string, int> OutfitsByOccasion { get; set; } = new();
}
