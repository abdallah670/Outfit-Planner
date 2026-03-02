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
}
