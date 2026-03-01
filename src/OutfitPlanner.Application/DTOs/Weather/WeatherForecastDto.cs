namespace OutfitPlanner.Application.DTOs.Weather;

public class WeatherForecastDto
{
    public DateOnly Date { get; set; }
    public double HighTemp { get; set; }
    public double LowTemp { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Description { get; set; } = string.Empty;
}
