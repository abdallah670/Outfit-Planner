namespace OutfitPlanner.Infrastructure.Configuration;

public class WeatherApiSettings
{
    public const string SectionName = "WeatherApi";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";
    public string Units { get; set; } = "metric";
    public int TimeoutSeconds { get; set; } = 30;
}
