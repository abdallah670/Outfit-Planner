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
        // Validate that either city or coordinates are provided
        if (string.IsNullOrWhiteSpace(request.City) && 
            (!request.Latitude.HasValue || !request.Longitude.HasValue))
        {
            throw new BadRequestException("Either city name or latitude/longitude coordinates must be provided");
        }

        var weather = await _weatherService.GetCurrentWeatherAsync(
            request.City,
            request.Latitude,
            request.Longitude,
            cancellationToken);

        if (weather == null)
        {
            throw new NotFoundException("Weather data", request.City ?? $"{request.Latitude}, {request.Longitude}");
        }

        return weather;
    }
}
