namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IServiceManagementService
{
    /// <summary>
    /// Gets the health status of all services
    /// </summary>
    Task<ServiceHealthStatus> GetServiceHealthAsync();

    /// <summary>
    /// Gets health status for a specific service
    /// </summary>
    Task<ServiceStatus> GetServiceStatusAsync(string serviceName);

    /// <summary>
    /// Restarts a specific service
    /// </summary>
    Task<ServiceRestartResult> RestartServiceAsync(string serviceName);

    /// <summary>
    /// Gets service metrics and performance data
    /// </summary>
    Task<ServiceMetrics> GetServiceMetricsAsync(string serviceName);

    /// <summary>
    /// Gets all available services
    /// </summary>
    Task<IEnumerable<ServiceInfo>> GetAvailableServicesAsync();

    /// <summary>
    /// Enables or disables a service
    /// </summary>
    Task<bool> ToggleServiceAsync(string serviceName, bool enabled);

    /// <summary>
    /// Gets service logs
    /// </summary>
    Task<IEnumerable<ServiceLogEntry>> GetServiceLogsAsync(string serviceName, int? limit = null);

    /// <summary>
    /// Performs health check on all services
    /// </summary>
    Task<HealthCheckResult> PerformHealthCheckAsync();
}

public record ServiceHealthStatus(
    DateTime CheckedAt,
    bool OverallHealthy,
    int TotalServices,
    int HealthyServices,
    int UnhealthyServices,
    int DisabledServices,
    IEnumerable<ServiceStatus> Services
);

public record ServiceStatus(
    string Name,
    bool IsHealthy,
    bool IsEnabled,
    ServiceType Type,
    DateTime LastChecked,
    TimeSpan ResponseTime,
    string? StatusMessage = null,
    int? ProcessId = null,
    long? MemoryUsage = null,
    double? CpuUsage = null
);

public record ServiceInfo(
    string Name,
    ServiceType Type,
    string Description,
    bool IsEnabled,
    bool IsRequired,
    string Version,
    DateTime StartedAt,
    TimeSpan Uptime
);

public record ServiceMetrics(
    string ServiceName,
    DateTime Timestamp,
    long MemoryUsage,
    double CpuUsage,
    int ActiveConnections,
    int RequestsPerSecond,
    TimeSpan AverageResponseTime,
    int ThreadCount,
    long TotalRequests,
    int ErrorCount
);

public record ServiceRestartResult(
    bool Success,
    string ServiceName,
    DateTime RestartedAt,
    TimeSpan Downtime,
    string? ErrorMessage = null
);

public record ServiceLogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Message,
    string? Category = null,
    string? Exception = null
);

public record HealthCheckResult(
    bool IsHealthy,
    DateTime CheckedAt,
    TimeSpan TotalDuration,
    IEnumerable<ServiceHealthCheck> ServiceChecks,
    string? Summary = null
);

public record ServiceHealthCheck(
    string ServiceName,
    bool IsHealthy,
    TimeSpan ResponseTime,
    string? Message = null
);

public enum ServiceType
{
    Database,
    Cache,
    Queue,
    Api,
    Background,
    External,
    Internal
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
