using FluentAssertions;
using Moq;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Weather.Handlers.Queries;
using OutfitPlanner.Application.Features.Weather.Requests.Queries;

namespace OutfitPlanner.Application.UnitTests.Weather;

public class GetCurrentWeatherQueryHandlerTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly GetCurrentWeatherQueryHandler _handler;

    public GetCurrentWeatherQueryHandlerTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _handler = new GetCurrentWeatherQueryHandler(_weatherServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithCityName_ReturnsWeather()
    {
        // Arrange
        var expectedWeather = new WeatherDto
        {
            City = "Cairo",
            Temperature = 25.5,
            Condition = "Clear",
            Description = "Clear sky",
            Humidity = 60,
            WindSpeed = 3.5,
            Icon = "01d",
            FeelsLike = 26.0,
            HighTemp = 28.0,
            LowTemp = 22.0
        };

        _weatherServiceMock
            .Setup(x => x.GetCurrentWeatherAsync("Cairo", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWeather);

        var query = new GetCurrentWeatherQuery { City = "Cairo" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.City.Should().Be("Cairo");
        result.Temperature.Should().Be(25.5);
        _weatherServiceMock.Verify(x => x.GetCurrentWeatherAsync("Cairo", null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCoordinates_ReturnsWeather()
    {
        // Arrange
        var expectedWeather = new WeatherDto
        {
            City = "London",
            Temperature = 15.0,
            Condition = "Clouds",
            Description = "Scattered clouds",
            Humidity = 75,
            WindSpeed = 5.2,
            Icon = "03d",
            FeelsLike = 14.0,
            HighTemp = 17.0,
            LowTemp = 12.0
        };

        _weatherServiceMock
            .Setup(x => x.GetCurrentWeatherAsync(null, 51.5074, -0.1278, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWeather);

        var query = new GetCurrentWeatherQuery { Latitude = 51.5074, Longitude = -0.1278 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.City.Should().Be("London");
        _weatherServiceMock.Verify(x => x.GetCurrentWeatherAsync(null, 51.5074, -0.1278, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsNull_ThrowsNotFoundException()
    {
        // Arrange
        _weatherServiceMock
            .Setup(x => x.GetCurrentWeatherAsync(It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherDto?)null);

        var query = new GetCurrentWeatherQuery { City = "UnknownCity" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
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
        var query = new GetCurrentWeatherQuery
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
    public async Task Handle_PassesCancellationToken_ToService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var expectedWeather = new WeatherDto { City = "Paris" };

        _weatherServiceMock
            .Setup(x => x.GetCurrentWeatherAsync("Paris", null, null, cts.Token))
            .ReturnsAsync(expectedWeather);

        var query = new GetCurrentWeatherQuery { City = "Paris" };

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _weatherServiceMock.Verify(x => x.GetCurrentWeatherAsync("Paris", null, null, cts.Token), Times.Once);
    }
}
