using MediatR;
using OutfitPlanner.Application.DTOs.Weather;

namespace OutfitPlanner.Application.Features.Weather.Requests.Queries;

/// <summary>
/// Query to get weather forecast for a specific month
/// </summary>
public class GetWeatherForMonthQuery : IRequest<List<WeatherForecastDto>>
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string? City { get; set; } = "Cairo"; // Default city
}
