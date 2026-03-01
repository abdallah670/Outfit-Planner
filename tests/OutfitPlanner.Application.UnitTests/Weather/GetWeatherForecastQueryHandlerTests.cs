using FluentAssertions;
using Moq;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Weather.Handlers.Queries;
using OutfitPlanner.Application.Features.Weather.Requests.Queries;

namespace OutfitPlanner.Application.UnitTests.Weather;

public class GetWeatherForecastQueryHandlerTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly GetWeatherForecastQueryHandler _handler;

    public GetWeatherForecastQueryHandlerTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _handler = new GetWeatherForecastQueryHandler(_weatherServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithCityName_ReturnsForecast()
    {
        // Arrange
        var expectedForecast = new List<WeatherForecastDto>
        {
            new() {
                Date = DateOnly.FromDateTime(DateTime.Today),
                HighTemp = 28.0,
                LowTemp = 18.0,
                Condition = "Clear",
                Description = "Sunny",
                Humidity = 55,
                WindSpeed = 3.5,
                Icon = "01d"
            },
            new() {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                HighTemp = 26.0,
                LowTemp = 17.0,
                Condition = "Clouds",
                Description = "Partly cloudy",
                Humidity = 60,
                WindSpeed = 4.2,
                Icon = "03d"
            }
        };

        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync("Cairo", null, null, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedForecast);

        var query = new GetWeatherForecastQuery { City = "Cairo" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Condition.Should().Be("Clear");
        _weatherServiceMock.Verify(x => x.GetWeatherForecastAsync("Cairo", null, null, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCoordinates_ReturnsForecast()
    {
        // Arrange
        var expectedForecast = new List<WeatherForecastDto>
        {
            new() {
                Date = DateOnly.FromDateTime(DateTime.Today),
                HighTemp = 15.0,
                LowTemp = 8.0,
                Condition = "Rain",
                Description = "Light rain",
                Icon = "10d"
            }
        };

        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync(null, 51.5074, -0.1278, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedForecast);

        var query = new GetWeatherForecastQuery { Latitude = 51.5074, Longitude = -0.1278 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _weatherServiceMock.Verify(x => x.GetWeatherForecastAsync(null, 51.5074, -0.1278, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomDays_PassesDaysToService()
    {
        // Arrange
        var expectedForecast = new List<WeatherForecastDto>();
        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync("Tokyo", null, null, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedForecast);

        var query = new GetWeatherForecastQuery { City = "Tokyo", Days = 10 };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _weatherServiceMock.Verify(x => x.GetWeatherForecastAsync("Tokyo", null, null, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 1)]   // Below minimum should clamp to 1
    [InlineData(-5, 1)]  // Negative should clamp to 1
    [InlineData(20, 16)] // Above maximum should clamp to 16
    [InlineData(100, 16)] // Way above maximum should clamp to 16
    public async Task Handle_WithOutOfRangeDays_ClampsToValidRange(int inputDays, int expectedDays)
    {
        // Arrange
        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync("City", null, null, expectedDays, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WeatherForecastDto>());

        var query = new GetWeatherForecastQuery { City = "City", Days = inputDays };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _weatherServiceMock.Verify(x => x.GetWeatherForecastAsync("City", null, null, expectedDays, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsEmptyList_ReturnsEmptyList()
    {
        // Arrange
        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync(It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WeatherForecastDto>());

        var query = new GetWeatherForecastQuery { City = "UnknownCity" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("", null, null)]
    [InlineData("   ", null, null)]
    [InlineData(null, null, 10.0)]
    [InlineData(null, 10.0, null)]
    public async Task Handle_WithInvalidParameters_ThrowsBadRequestException(string? city, double? lat, double? lon)
    {
        // Arrange
        var query = new GetWeatherForecastQuery
        {
            City = city,
            Latitude = lat,
            Longitude = lon
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Contain("Either city name or latitude/longitude coordinates must be provided");
    }

    [Fact]
    public async Task Handle_WithValidDaysInRange_PassesOriginalValue()
    {
        // Arrange
        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync("Sydney", null, null, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WeatherForecastDto>());

        var query = new GetWeatherForecastQuery { City = "Sydney", Days = 7 };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _weatherServiceMock.Verify(x => x.GetWeatherForecastAsync("Sydney", null, null, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PassesCancellationToken_ToService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _weatherServiceMock
            .Setup(x => x.GetWeatherForecastAsync("Berlin", null, null, 5, cts.Token))
            .ReturnsAsync(new List<WeatherForecastDto>());

        var query = new GetWeatherForecastQuery { City = "Berlin" };

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _weatherServiceMock.Verify(x => x.GetWeatherForecastAsync("Berlin", null, null, 5, cts.Token), Times.Once);
    }
}
