namespace OutfitPlanner.Application.DTOs.Wardrobe;

public class WardrobeHealthDto
{
    /// <summary>
    /// Percentage of wardrobe items worn at least once (0-100)
    /// </summary>
    public int HealthPercentage { get; set; }

    /// <summary>
    /// Total number of items in wardrobe
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items worn at least once
    /// </summary>
    public int WornItems { get; set; }

    /// <summary>
    /// Number of items never worn
    /// </summary>
    public int UnwornItems { get; set; }

    /// <summary>
    /// Total wear count across all items
    /// </summary>
    public int TotalWears { get; set; }

    /// <summary>
    /// Average wears per item
    /// </summary>
    public double AverageWearsPerItem { get; set; }

    /// <summary>
    /// Most worn item name
    /// </summary>
    public string? MostWornItemName { get; set; }

    /// <summary>
    /// Wear count of most worn item
    /// </summary>
    public int MostWornItemCount { get; set; }
}
