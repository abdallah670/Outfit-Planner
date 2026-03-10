namespace OutfitPlanner.Application.DTOs.Calendar;

/// <summary>
/// Request to mark a scheduled outfit as worn
/// </summary>
public class MarkAsWornRequest
{
    public int DurationMinutes { get; set; }
    public string? WeatherCondition { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
}
