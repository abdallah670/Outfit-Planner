namespace OutfitPlanner.Domain.Entities;

public class AppPreferences : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    
    // General Preferences
    public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
    public string Language { get; set; } = "en";
    public AppTheme Theme { get; set; } = AppTheme.Auto;
    public MeasurementUnit MeasurementUnit { get; set; } = MeasurementUnit.Metric;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit
}

public enum AppTheme
{
    Light,
    Dark,
    Auto
}

public enum MeasurementUnit
{
    Metric,
    Imperial
}
