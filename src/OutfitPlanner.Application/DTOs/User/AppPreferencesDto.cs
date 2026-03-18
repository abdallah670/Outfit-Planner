namespace OutfitPlanner.Application.DTOs.User;

public class AppPreferencesDto
{
    public string TemperatureUnit { get; set; } = "Celsius";
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "Auto";
    public string MeasurementUnit { get; set; } = "Metric";
}

public class UpdateAppPreferencesDto
{
    public string TemperatureUnit { get; set; } = "Celsius";
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "Auto";
    public string MeasurementUnit { get; set; } = "Metric";
}
