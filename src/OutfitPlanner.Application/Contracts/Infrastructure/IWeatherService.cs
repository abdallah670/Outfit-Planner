using OutfitPlanner.Application.DTOs.Weather;

namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IWeatherService
{
    Task<WeatherDto?> GetCurrentWeatherAsync(
        string? city = null,
        double? latitude = null,
        double? longitude = null,
        CancellationToken cancellationToken = default);

    Task<List<WeatherForecastDto>> GetWeatherForecastAsync(
        string? city = null,
        double? latitude = null,
        double? longitude = null,
        int days = 5,
        CancellationToken cancellationToken = default);
}
