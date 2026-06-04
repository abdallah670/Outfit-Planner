using MediatR;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Weather.Requests.Queries;

namespace OutfitPlanner.Application.Features.Weather.Handlers.Queries;

public class GetCurrentWeatherQueryHandler : IRequestHandler<GetCurrentWeatherQuery, WeatherDto>
{
    private readonly IWeatherService _weatherService;

    public GetCurrentWeatherQueryHandler(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<WeatherDto> Handle(GetCurrentWeatherQuery request, CancellationToken cancellationToken)
    {
        var city = request.City;
        double? lat = request.Latitude;
        double? lon = request.Longitude;
        
        if (string.IsNullOrWhiteSpace(city) && (!lat.HasValue || !lon.HasValue))
        {
            city = "Cairo";
        }

        var weather = await _weatherService.GetCurrentWeatherAsync(
            city,
            lat,
            lon,
            cancellationToken);

        if (weather == null)
        {
            throw new NotFoundException("Weather data", request.City ?? $"{request.Latitude}, {request.Longitude}");
        }

        return weather;
    }
}
