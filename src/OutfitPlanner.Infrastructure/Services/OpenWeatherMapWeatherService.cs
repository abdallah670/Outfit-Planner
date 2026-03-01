using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Infrastructure.Configuration;
using OutfitPlanner.Infrastructure.Services.Models;

namespace OutfitPlanner.Infrastructure.Services;

public class OpenWeatherMapWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;
    private readonly ILogger<OpenWeatherMapWeatherService> _logger;

    public OpenWeatherMapWeatherService(
        HttpClient httpClient,
        IOptions<WeatherApiSettings> settings,
        ILogger<OpenWeatherMapWeatherService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WeatherDto?> GetCurrentWeatherAsync(
        string? city = null,
        double? latitude = null,
        double? longitude = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildQuery(city, latitude, longitude);
            var url = $"{_settings.BaseUrl}/weather?{query}&appid={_settings.ApiKey}&units={_settings.Units}";

            _logger.LogInformation("Fetching current weather from: {Url}", url.Replace(_settings.ApiKey, "***"));

            var response = await _httpClient.GetFromJsonAsync<OpenWeatherCurrentResponse>(url, cancellationToken);

            if (response == null)
            {
                _logger.LogWarning("No weather data received from API");
                return null;
            }

            return MapToWeatherDto(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching weather data");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching weather data");
            return null;
        }
    }

    public async Task<List<WeatherForecastDto>> GetWeatherForecastAsync(
        string? city = null,
        double? latitude = null,
        double? longitude = null,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // OpenWeatherMap forecast API returns 3-hour intervals for 5 days
            // We'll aggregate them into daily forecasts
            var query = BuildQuery(city, latitude, longitude);
            var url = $"{_settings.BaseUrl}/forecast?{query}&appid={_settings.ApiKey}&units={_settings.Units}";

            _logger.LogInformation("Fetching weather forecast from: {Url}", url.Replace(_settings.ApiKey, "***"));

            var response = await _httpClient.GetFromJsonAsync<OpenWeatherForecastResponse>(url, cancellationToken);

            if (response?.List == null || response.List.Count == 0)
            {
                _logger.LogWarning("No forecast data received from API");
                return new List<WeatherForecastDto>();
            }

            var dailyForecasts = AggregateToDailyForecasts(response.List, days);
            return dailyForecasts;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching forecast data");
            return new List<WeatherForecastDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching forecast data");
            return new List<WeatherForecastDto>();
        }
    }

    private string BuildQuery(string? city, double? latitude, double? longitude)
    {
        if (!string.IsNullOrWhiteSpace(city))
        {
            return $"q={Uri.EscapeDataString(city)}";
        }

        if (latitude.HasValue && longitude.HasValue)
        {
            return $"lat={latitude.Value}&lon={longitude.Value}";
        }

        // Default fallback - San Francisco
        _logger.LogWarning("No location provided, using default city (San Francisco)");
        return "q=San Francisco";
    }

    private WeatherDto MapToWeatherDto(OpenWeatherCurrentResponse response)
    {
        var weather = response.Weather?.FirstOrDefault();

        return new WeatherDto
        {
            Temperature = response.Main?.Temp ?? 0,
            Condition = weather?.Main ?? "Unknown",
            Description = CapitalizeFirst(weather?.Description ?? "No description available"),
            Humidity = response.Main?.Humidity ?? 0,
            WindSpeed = response.Wind?.Speed ?? 0,
            Icon = weather?.Icon ?? "01d",
            City = response.Name ?? "Unknown",
            FeelsLike = response.Main?.FeelsLike ?? 0,
            HighTemp = response.Main?.TempMax ?? 0,
            LowTemp = response.Main?.TempMin ?? 0
        };
    }

    private List<WeatherForecastDto> AggregateToDailyForecasts(List<OpenWeatherForecastItem> items, int days)
    {
        var groupedByDay = items
            .GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).Date)
            .Take(days);

        var forecasts = new List<WeatherForecastDto>();

        foreach (var dayGroup in groupedByDay)
        {
            var dayItems = dayGroup.ToList();
            var mainWeather = dayItems
                .GroupBy(i => i.Weather?.FirstOrDefault()?.Main)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()
                ?.FirstOrDefault()
                ?.Weather?.FirstOrDefault();

            var forecast = new WeatherForecastDto
            {
                Date = DateOnly.FromDateTime(dayGroup.Key),
                HighTemp = dayItems.Max(i => i.Main?.TempMax ?? 0),
                LowTemp = dayItems.Min(i => i.Main?.TempMin ?? 0),
                Condition = mainWeather?.Main ?? "Unknown",
                Description = CapitalizeFirst(mainWeather?.Description ?? "No description available"),
                Icon = mainWeather?.Icon ?? "01d",
                Humidity = (int)dayItems.Average(i => i.Main?.Humidity ?? 0),
                WindSpeed = dayItems.Average(i => i.Wind?.Speed ?? 0)
            };

            forecasts.Add(forecast);
        }

        return forecasts;
    }

    private static string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input[1..];
    }
}
