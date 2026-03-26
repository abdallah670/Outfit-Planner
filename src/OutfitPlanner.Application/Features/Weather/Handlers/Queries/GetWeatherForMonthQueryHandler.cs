using MediatR;
using OutfitPlanner.Application.DTOs.Weather;
using OutfitPlanner.Application.Features.Weather.Requests.Queries;

namespace OutfitPlanner.Application.Features.Weather.Handlers.Queries;

/// <summary>
/// Handler for getting weather forecast for a specific month
/// Returns simulated data for each day of the month
/// </summary>
public class GetWeatherForMonthQueryHandler : IRequestHandler<GetWeatherForMonthQuery, List<WeatherForecastDto>>
{
    public Task<List<WeatherForecastDto>> Handle(GetWeatherForMonthQuery request, CancellationToken cancellationToken)
    {
        var forecasts = new List<WeatherForecastDto>();
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        
        var random = new Random(request.Year * 100 + request.Month); // Seeded for consistency
        
        // Weather patterns based on month (Northern Hemisphere)
        var (baseTemp, commonConditions) = GetWeatherPatternForMonth(request.Month);
        
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(request.Year, request.Month, day);
            
            // Simulate temperature variation
            var tempVariation = random.Next(-5, 6); // -5 to +5 degrees variation
            var highTemp = baseTemp + tempVariation + random.Next(3, 8);
            var lowTemp = baseTemp + tempVariation - random.Next(3, 8);
            
            // Select weather condition
            var condition = commonConditions[random.Next(commonConditions.Length)];
            var (icon, description) = GetWeatherIconAndDescription(condition, date);
            
            forecasts.Add(new WeatherForecastDto
            {
                Date = DateOnly.FromDateTime(date),
                HighTemp = highTemp,
                LowTemp = lowTemp,
                Condition = condition,
                Icon = icon,
                Description = description,
                Humidity = random.Next(40, 90),
                WindSpeed = random.Next(5, 25)
            });
        }
        
        return Task.FromResult(forecasts);
    }
    
    private (int baseTemp, string[] conditions) GetWeatherPatternForMonth(int month)
    {
        return month switch
        {
            12 or 1 or 2 => (15, new[] { "Sunny", "Cloudy", "Partly Cloudy" }), // Winter
            3 or 4 or 5 => (22, new[] { "Sunny", "Partly Cloudy", "Cloudy" }), // Spring
            6 or 7 or 8 => (30, new[] { "Sunny", "Hot", "Partly Cloudy" }), // Summer
            9 or 10 or 11 => (25, new[] { "Sunny", "Partly Cloudy", "Cloudy" }), // Autumn
            _ => (20, new[] { "Sunny", "Cloudy" })
        };
    }
    
    private (string icon, string description) GetWeatherIconAndDescription(string condition, DateTime date)
    {
        var isDay = date.Hour >= 6 && date.Hour < 18;
        var dayNight = isDay ? "d" : "n";
        
        return condition switch
        {
            "Sunny" or "Hot" => ($"01{dayNight}", "Clear sky"),
            "Partly Cloudy" => ($"02{dayNight}", "Few clouds"),
            "Cloudy" => ($"03{dayNight}", "Scattered clouds"),
            "Rainy" => ($"10{dayNight}", "Light rain"),
            "Stormy" => ($"11{dayNight}", "Thunderstorm"),
            _ => ($"01{dayNight}", "Clear sky")
        };
    }
}
