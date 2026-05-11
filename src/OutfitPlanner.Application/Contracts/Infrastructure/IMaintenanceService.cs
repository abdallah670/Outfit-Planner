namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IMaintenanceService
{
    /// <summary>
    /// Gets the current maintenance mode status
    /// </summary>
    Task<MaintenanceStatus> GetMaintenanceStatusAsync();

    /// <summary>
    /// Enables maintenance mode with an optional message
    /// </summary>
    Task EnableMaintenanceModeAsync(string? message = null);

    /// <summary>
    /// Disables maintenance mode
    /// </summary>
    Task DisableMaintenanceModeAsync();

    /// <summary>
    /// Checks if maintenance mode is currently active
    /// </summary>
    Task<bool> IsMaintenanceModeActiveAsync();

    /// <summary>
    /// Updates the maintenance mode message
    /// </summary>
    Task UpdateMaintenanceMessageAsync(string message);

    /// <summary>
    /// Gets the maintenance mode configuration
    /// </summary>
    Task<MaintenanceConfiguration> GetMaintenanceConfigurationAsync();

    /// <summary>
    /// Updates maintenance mode configuration
    /// </summary>
    Task UpdateMaintenanceConfigurationAsync(MaintenanceConfiguration configuration);
}

public record MaintenanceStatus(
    bool IsEnabled,
    string? Message,
    DateTime EnabledAt,
    string? EnabledBy,
    DateTime? ScheduledUntil
);

public record MaintenanceConfiguration(
    bool AllowAdminAccess,
    bool ShowCountdown,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    string[] BypassIps,
    string[] AllowedPaths
);
