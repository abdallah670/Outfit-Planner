using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.DTOs.Weather;

namespace OutfitPlanner.Application.Responses;

public class TodaysPickResult
{
    public OutfitDto? Outfit { get; set; }
    public WeatherContextDto? WeatherContext { get; set; }
    public TodayEventDto? TodayEvent { get; set; }
    public int MatchScore { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
    public bool IsBestEffort { get; set; }
}
