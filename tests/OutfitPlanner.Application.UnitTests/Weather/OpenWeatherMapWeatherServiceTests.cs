using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Infrastructure.Configuration;
using OutfitPlanner.Infrastructure.Services;
using System.Net;
using System.Text.Json;

namespace OutfitPlanner.Application.UnitTests.Weather;

public class OpenWeatherMapWeatherServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOptions<WeatherApiSettings>> _settingsMock;
    private readonly Mock<ILogger<OpenWeatherMapWeatherService>> _loggerMock;
    private readonly OpenWeatherMapWeatherService _service;

    public OpenWeatherMapWeatherServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.openweathermap.org/data/2.5")
        };
        
        _settingsMock = new Mock<IOptions<WeatherApiSettings>>();
        _settingsMock.Setup(x => x.Value).Returns(new WeatherApiSettings
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.openweathermap.org/data/2.5",
            Units = "metric",
            TimeoutSeconds = 30
        });
        
        _loggerMock = new Mock<ILogger<OpenWeatherMapWeatherService>>();
        _service = new OpenWeatherMapWeatherService(_httpClient, _settingsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithCity_ReturnsMappedWeather()
    {
        // Arrange
        var apiResponse = new
        {
            coord = new { lon = 31.2497, lat = 30.0626 },
            weather = new[] { new { id = 800, main = "Clear", description = "clear sky", icon = "01d" } },
            main = new { temp = 25.5, feels_like = 26.0, temp_min = 22.0, temp_max = 28.0, pressure = 1012, humidity = 60 },
            wind = new { speed = 3.5, deg = 200 },
            name = "Cairo",
            dt = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        SetupHttpResponse("/weather*", HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _service.GetCurrentWeatherAsync("Cairo");

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be("Cairo");
        result.Temperature.Should().Be(25.5);
        result.Condition.Should().Be("Clear");
        result.Description.Should().Be("Clear sky");
        result.Humidity.Should().Be(60);
        result.WindSpeed.Should().Be(3.5);
        result.Icon.Should().Be("01d");
        result.FeelsLike.Should().Be(26.0);
        result.HighTemp.Should().Be(28.0);
        result.LowTemp.Should().Be(22.0);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithCoordinates_ReturnsMappedWeather()
    {
        // Arrange
        var apiResponse = new
        {
            coord = new { lon = -0.1278, lat = 51.5074 },
            weather = new[] { new { id = 803, main = "Clouds", description = "broken clouds", icon = "04d" } },
            main = new { temp = 15.0, feels_like = 14.0, temp_min = 12.0, temp_max = 17.0, pressure = 1015, humidity = 75 },
            wind = new { speed = 5.2, deg = 270 },
            name = "London",
            dt = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        SetupHttpResponse("/weather*", HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _service.GetCurrentWeatherAsync(null, 51.5074, -0.1278);

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be("London");
        result.Condition.Should().Be("Clouds");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithNullCityAndNoCoordinates_UsesDefaultCity()
    {
        // Arrange
        var apiResponse = new
        {
            coord = new { lon = -122.4194, lat = 37.7749 },
            weather = new[] { new { id = 800, main = "Clear", description = "clear sky", icon = "01d" } },
            main = new { temp = 22.0, feels_like = 21.0, temp_min = 18.0, temp_max = 25.0, pressure = 1013, humidity = 65 },
            wind = new { speed = 4.0, deg = 300 },
            name = "San Francisco",
            dt = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        SetupHttpResponse("/weather*", HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _service.GetCurrentWeatherAsync(null, null, null);

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be("San Francisco");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WhenApiReturnsError_ReturnsNull()
    {
        // Arrange
        SetupHttpResponse("/weather*", HttpStatusCode.NotFound, null);

        // Act
        var result = await _service.GetCurrentWeatherAsync("InvalidCity");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WithCity_ReturnsDailyForecasts()
    {
        // Arrange
        var forecastItems = Enumerable.Range(0, 40).Select(i => new
        {
            dt = DateTimeOffset.Now.AddHours(i * 3).ToUnixTimeSeconds(),
            main = new { temp = 20.0 + i, temp_min = 18.0 + i, temp_max = 22.0 + i, humidity = 60 + (i % 10) },
            weather = new[] { new { id = 800, main = "Clear", description = "clear sky", icon = "01d" } },
            wind = new { speed = 3.0 + (i % 5), deg = 200 },
            dt_txt = DateTime.Now.AddHours(i * 3).ToString("yyyy-MM-dd HH:mm:ss")
        }).ToArray();

        var apiResponse = new
        {
            list = forecastItems,
            city = new { id = 360630, name = "Cairo", coord = new { lon = 31.2497, lat = 30.0626 }, country = "EG" }
        };

        SetupHttpResponse("/forecast*", HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _service.GetWeatherForecastAsync("Cairo", null, null, 5);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.First().HighTemp.Should().BeGreaterThan(0);
        result.First().LowTemp.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var apiResponse = new { list = Array.Empty<object>(), city = new { name = "EmptyCity" } };
        SetupHttpResponse("/forecast*", HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _service.GetWeatherForecastAsync("EmptyCity");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenApiReturnsError_ReturnsEmptyList()
    {
        // Arrange
        SetupHttpResponse("/forecast*", HttpStatusCode.BadRequest, null);

        // Act
        var result = await _service.GetWeatherForecastAsync("InvalidCity");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private void SetupHttpResponse(string requestUri, HttpStatusCode statusCode, object? responseContent)
    {
        var response = new HttpResponseMessage(statusCode);
        
        if (responseContent != null)
        {
            var json = JsonSerializer.Serialize(responseContent);
            response.Content = new StringContent(json);
        }

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.PathAndQuery.Contains(requestUri.TrimEnd('*'))),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}
