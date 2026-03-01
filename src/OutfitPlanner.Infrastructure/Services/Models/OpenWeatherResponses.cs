using System.Text.Json.Serialization;

namespace OutfitPlanner.Infrastructure.Services.Models;

// Current Weather Response
public class OpenWeatherCurrentResponse
{
    [JsonPropertyName("coord")]
    public Coordinates? Coord { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherDescription>? Weather { get; set; }

    [JsonPropertyName("main")]
    public MainWeather? Main { get; set; }

    [JsonPropertyName("wind")]
    public WindInfo? Wind { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("dt")]
    public long Dt { get; set; }
}

// Forecast Response
public class OpenWeatherForecastResponse
{
    [JsonPropertyName("list")]
    public List<OpenWeatherForecastItem>? List { get; set; }

    [JsonPropertyName("city")]
    public CityInfo? City { get; set; }
}

public class OpenWeatherForecastItem
{
    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public MainWeather? Main { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherDescription>? Weather { get; set; }

    [JsonPropertyName("wind")]
    public WindInfo? Wind { get; set; }

    [JsonPropertyName("dt_txt")]
    public string? DtTxt { get; set; }
}

// Shared Models
public class Coordinates
{
    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }
}

public class WeatherDescription
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("main")]
    public string? Main { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}

public class MainWeather
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("temp_min")]
    public double TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public double TempMax { get; set; }

    [JsonPropertyName("pressure")]
    public int Pressure { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

public class WindInfo
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("deg")]
    public int Deg { get; set; }
}

public class CityInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("coord")]
    public Coordinates? Coord { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
