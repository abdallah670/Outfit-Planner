using MediatR;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Weather.Requests.Queries;

namespace OutfitPlanner.Application.Features.Weather.Handlers.Queries;

public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, List<WeatherForecastDto>>
{
    private readonly IWeatherService _weatherService;

    public GetWeatherForecastQueryHandler(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<List<WeatherForecastDto>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        // Validate that either city or coordinates are provided
        if (string.IsNullOrWhiteSpace(request.City) && 
            (!request.Latitude.HasValue || !request.Longitude.HasValue))
        {
            throw new BadRequestException("Either city name or latitude/longitude coordinates must be provided");
        }

        // Limit days to valid range (1-16 for OpenWeatherMap free tier)
        var days = Math.Clamp(request.Days, 1, 16);

        var forecast = await _weatherService.GetWeatherForecastAsync(
            request.City,
            request.Latitude,
            request.Longitude,
            days,
            cancellationToken);

        return forecast;
    }
}
