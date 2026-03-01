using MediatR;
using OutfitPlanner.Application.DTOs.Weather;

namespace OutfitPlanner.Application.Features.Weather.Requests.Queries;

public class GetWeatherForecastQuery : IRequest<List<WeatherForecastDto>>
{
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Days { get; set; } = 5;
}
