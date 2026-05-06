using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Features.Weather.Requests.Queries;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IMediator mediator, ILogger<WeatherController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current weather for a location
    /// </summary>
    /// <param name="city">City name (optional if lat/lon provided)</param>
    /// <param name="lat">Latitude (optional if city provided)</param>
    /// <param name="lon">Longitude (optional if city provided)</param>
    [HttpGet("current")]
    [ProducesResponseType(typeof(WeatherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherDto>> GetCurrentWeather(
        [FromQuery] string? city = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null)
    {
        _logger.LogInformation("Getting current weather for City: {City}, Lat: {Lat}, Lon: {Lon}", 
            city ?? "(not provided)", lat, lon);

        var query = new GetCurrentWeatherQuery
        {
            City = city,
            Latitude = lat,
            Longitude = lon
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get weather forecast for a location
    /// </summary>
    /// <param name="city">City name (optional if lat/lon provided)</param>
    /// <param name="lat">Latitude (optional if city provided)</param>
    /// <param name="lon">Longitude (optional if city provided)</param>
    /// <param name="days">Number of forecast days (1-16, default: 5)</param>
    [HttpGet("forecast")]
    [ProducesResponseType(typeof(List<WeatherForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<WeatherForecastDto>>> GetWeatherForecast(
        [FromQuery] string? city = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] int days = 5)
    {
        _logger.LogInformation("Getting weather forecast for City: {City}, Lat: {Lat}, Lon: {Lon}, Days: {Days}", 
            city ?? "(not provided)", lat, lon, days);

        var query = new GetWeatherForecastQuery
        {
            City = city,
            Latitude = lat,
            Longitude = lon,
            Days = days
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get weather forecast for a specific month (for calendar display)
    /// </summary>
    /// <param name="year">Year (e.g., 2026)</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="city">City name (optional, defaults to Cairo)</param>
    [HttpGet("forecast/month")]
    [ProducesResponseType(typeof(List<WeatherForecastDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WeatherForecastDto>>> GetWeatherForMonth(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string? city = null)
    {
        _logger.LogInformation("Getting weather forecast for {Year}-{Month}, City: {City}", 
            year, month, city ?? "default");

        var query = new GetWeatherForMonthQuery
        {
            Year = year,
            Month = month,
            City = city ?? "Cairo"
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get weather forecast for a specific date
    /// </summary>
    /// <param name="date">Date in format YYYY-MM-DD</param>
    /// <param name="city">City name (optional, defaults to Cairo)</param>
    /// <param name="lat">Latitude (optional)</param>
    /// <param name="lon">Longitude (optional)</param>
    [HttpGet("forecast/by-date")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherForecastDto>> GetWeatherByDate(
        [FromQuery] string date,
        [FromQuery] string? city = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null)
    {
        _logger.LogInformation("Getting weather forecast for date: {Date}, City: {City}", 
            date, city ?? "default");

        // Parse the date
        if (!DateTime.TryParse(date, out var targetDate))
        {
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        }

        // Get forecast for the date range
        var query = new GetWeatherForecastQuery
        {
            City = city ?? "Cairo",
            Latitude = lat,
            Longitude = lon,
            Days = 16 // Get enough days to cover the target date
        };

        var forecasts = await _mediator.Send(query);
        
        // Find the forecast for the specific date
        var targetForecast = forecasts?.FirstOrDefault(f => f.Date == DateOnly.FromDateTime(targetDate));
        
        if (targetForecast == null)
        {
            return NotFound($"Weather forecast not available for date: {date}");
        }

        return Ok(targetForecast);
    }
}
