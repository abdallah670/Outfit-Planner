namespace OutfitPlanner.Application.DTOs.Weather;

public class WeatherDto
{
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double FeelsLike { get; set; }
    public double HighTemp { get; set; }
    public double LowTemp { get; set; }
}
public class WeatherContextDto
{
    public string Condition { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Season { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
