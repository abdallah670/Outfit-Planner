
namespace OutfitPlanner.Application.DTOs.Outfit;

public class OutfitSuggestionsDto
{
   

    /// <summary>
    /// Optional occasion filter (e.g., "Casual", "Formal", "Business").
    /// </summary>
    public string? Occasion { get; set; }

    /// <summary>
    /// Optional season filter (e.g., "Spring", "Summer", "Fall", "Winter").
    /// </summary>
    public string? Season { get; set; }

    /// <summary>
    /// Optional weather condition filter (e.g., "Sunny", "Rainy", "Cold").
    /// </summary>
    public string? WeatherCondition { get; set; }

    /// <summary>
    /// Maximum number of outfit suggestions to return. Defaults to 10.
    /// </summary>
    public int MaxSuggestions { get; set; } = 10;
}