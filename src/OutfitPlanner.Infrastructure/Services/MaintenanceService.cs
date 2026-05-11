using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;

namespace OutfitPlanner.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MaintenanceService> _logger;
    private const string MaintenanceStatusKey = "maintenance_status";
    private const string MaintenanceConfigKey = "maintenance_config";

    public MaintenanceService(IMemoryCache memoryCache, ILogger<MaintenanceService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<MaintenanceStatus> GetMaintenanceStatusAsync()
    {
        try
        {
            var status = await Task.FromResult(_memoryCache.Get<MaintenanceStatus>(MaintenanceStatusKey));
            
            if (status == null)
            {
                // Default status - maintenance mode is disabled
                status = new MaintenanceStatus(false, null, DateTime.MinValue, null, null);
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get maintenance status");
            throw;
        }
    }

    public async Task EnableMaintenanceModeAsync(string? message = null)
    {
        try
        {
            _logger.LogInformation("Enabling maintenance mode with message: {Message}", message);

            var status = new MaintenanceStatus(
                true,
                message ?? "System is currently under maintenance. Please try again later.",
                DateTime.UtcNow,
                "System",
                null
            );

            _memoryCache.Set(MaintenanceStatusKey, status, TimeSpan.FromDays(1));

            _logger.LogInformation("Maintenance mode enabled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable maintenance mode");
            throw;
        }
    }

    public async Task DisableMaintenanceModeAsync()
    {
        try
        {
            _logger.LogInformation("Disabling maintenance mode");

            var status = new MaintenanceStatus(false, null, DateTime.MinValue, null, null);
            _memoryCache.Set(MaintenanceStatusKey, status, TimeSpan.FromDays(1));

            _logger.LogInformation("Maintenance mode disabled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable maintenance mode");
            throw;
        }
    }

    public async Task<bool> IsMaintenanceModeActiveAsync()
    {
        try
        {
            var status = await GetMaintenanceStatusAsync();
            return await Task.FromResult(status.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check maintenance mode status");
            return await Task.FromResult(false);
        }
    }

    public async Task UpdateMaintenanceMessageAsync(string message)
    {
        try
        {
            _logger.LogInformation("Updating maintenance message to: {Message}", message);

            var currentStatus = await GetMaintenanceStatusAsync();
            if (currentStatus.IsEnabled)
            {
                var updatedStatus = currentStatus with { Message = message };
                _memoryCache.Set(MaintenanceStatusKey, updatedStatus, TimeSpan.FromDays(1));
                
                _logger.LogInformation("Maintenance message updated successfully");
            }
            else
            {
                _logger.LogWarning("Attempted to update maintenance message while maintenance mode is disabled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update maintenance message");
            throw;
        }
    }

    public async Task<MaintenanceConfiguration> GetMaintenanceConfigurationAsync()
    {
        try
        {
            var config = await Task.FromResult(_memoryCache.Get<MaintenanceConfiguration>(MaintenanceConfigKey));
            
            if (config == null)
            {
                // Default configuration
                config = new MaintenanceConfiguration(
                    true,  // Allow admin access
                    false, // Show countdown
                    null,  // Scheduled start
                    null,  // Scheduled end
                    new[] { "127.0.0.1", "::1" }, // Bypass localhost
                    new[] { "/health", "/admin/maintenance" } // Allowed paths
                );
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get maintenance configuration");
            throw;
        }
    }

    public async Task UpdateMaintenanceConfigurationAsync(MaintenanceConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Updating maintenance configuration");

            _memoryCache.Set(MaintenanceConfigKey, configuration, TimeSpan.FromDays(1));

            _logger.LogInformation("Maintenance configuration updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update maintenance configuration");
            throw;
        }
    }
}
