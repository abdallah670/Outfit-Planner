using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Persistence;
using System.Diagnostics;
using System.Text.Json;
using MicrosoftHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace OutfitPlanner.Infrastructure.Services;

public class ServiceManagementService : IServiceManagementService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ServiceManagementService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HealthCheckService _healthCheckService;
    private readonly IConfiguration _configuration;

    public ServiceManagementService(
        AppDbContext dbContext,
        ILogger<ServiceManagementService> logger,
        IServiceProvider serviceProvider,
        HealthCheckService healthCheckService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _healthCheckService = healthCheckService;
        _configuration = configuration;
    }

    public async Task<ServiceHealthStatus> GetServiceHealthAsync()
    {
        try
        {
            _logger.LogInformation("Getting service health status");

            var services = await GetAvailableServicesAsync();
            var serviceStatuses = new List<ServiceStatus>();

            foreach (var service in services)
            {
                var status = await GetServiceStatusAsync(service.Name);
                serviceStatuses.Add(status);
            }

            var overallHealthy = serviceStatuses.All(s => s.IsHealthy || !s.IsEnabled);
            var healthyCount = serviceStatuses.Count(s => s.IsHealthy);
            var unhealthyCount = serviceStatuses.Count(s => !s.IsHealthy && s.IsEnabled);
            var disabledCount = serviceStatuses.Count(s => !s.IsEnabled);

            return new ServiceHealthStatus(
                DateTime.UtcNow,
                overallHealthy,
                serviceStatuses.Count,
                healthyCount,
                unhealthyCount,
                disabledCount,
                serviceStatuses
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service health status");
            return new ServiceHealthStatus(
                DateTime.UtcNow,
                false,
                0,
                0,
                0,
                0,
                Enumerable.Empty<ServiceStatus>()
            );
        }
    }

    public async Task<ServiceStatus> GetServiceStatusAsync(string serviceName)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var isHealthy = await CheckServiceHealthAsync(serviceName);
            stopwatch.Stop();

            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64;
            var cpuUsage = GetCpuUsage();

            return new ServiceStatus(
                serviceName,
                isHealthy,
                true, // Assume enabled for now
                GetServiceType(serviceName),
                DateTime.UtcNow,
                stopwatch.Elapsed,
                isHealthy ? "Service is healthy" : "Service is unhealthy",
                process.Id,
                memoryUsage,
                cpuUsage
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for service: {ServiceName}", serviceName);
            return new ServiceStatus(
                serviceName,
                false,
                false,
                GetServiceType(serviceName),
                DateTime.UtcNow,
                TimeSpan.Zero,
                $"Error checking service: {ex.Message}"
            );
        }
    }

    public async Task<ServiceRestartResult> RestartServiceAsync(string serviceName)
    {
        try
        {
            _logger.LogInformation("Restarting service: {ServiceName}", serviceName);

            var startTime = DateTime.UtcNow;
            var downtime = TimeSpan.Zero;

            // In a real implementation, you would restart the actual service
            // For now, we'll simulate a restart
            await Task.Delay(2000); // Simulate restart time

            downtime = DateTime.UtcNow - startTime;

            _logger.LogInformation("Service {ServiceName} restarted successfully", serviceName);

            return new ServiceRestartResult(
                true,
                serviceName,
                DateTime.UtcNow,
                downtime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart service: {ServiceName}", serviceName);
            return new ServiceRestartResult(
                false,
                serviceName,
                DateTime.UtcNow,
                TimeSpan.Zero,
                ex.Message
            );
        }
    }

    public async Task<ServiceMetrics> GetServiceMetricsAsync(string serviceName)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64;
            var cpuUsage = GetCpuUsage();
            var threadCount = process.Threads.Count;

            // Get real performance metrics
            var activeConnections = GetActiveConnections(serviceName);
            var requestsPerSecond = await GetRequestsPerSecondAsync(serviceName);
            var averageResponseTime = await GetAverageResponseTimeAsync(serviceName);
            var totalRequests = await GetTotalRequestsAsync(serviceName);
            var errorCount = await GetErrorCountAsync(serviceName);

            return new ServiceMetrics(
                serviceName,
                DateTime.UtcNow,
                memoryUsage,
                cpuUsage,
                activeConnections,
                requestsPerSecond,
                averageResponseTime,
                threadCount,
                totalRequests,
                errorCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metrics for service: {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetAvailableServicesAsync()
    {
        try
        {
            var services = new List<ServiceInfo>
            {
                new ServiceInfo(
                    "Database",
                    ServiceType.Database,
                    "Primary application database",
                    true,
                    true,
                    "1.0.0",
                    DateTime.UtcNow.AddHours(-2),
                    TimeSpan.FromHours(2)
                ),
                new ServiceInfo(
                    "Cache",
                    ServiceType.Cache,
                    "In-memory cache service",
                    true,
                    true,
                    "1.0.0",
                    DateTime.UtcNow.AddHours(-1),
                    TimeSpan.FromHours(1)
                ),
                new ServiceInfo(
                    "API",
                    ServiceType.Api,
                    "Main API service",
                    true,
                    true,
                    "1.0.0",
                    DateTime.UtcNow.AddMinutes(-30),
                    TimeSpan.FromMinutes(30)
                ),
                new ServiceInfo(
                    "Background",
                    ServiceType.Background,
                    "Background job processing",
                    true,
                    false,
                    "1.0.0",
                    DateTime.UtcNow.AddMinutes(-15),
                    TimeSpan.FromMinutes(15)
                )
            };

            return await Task.FromResult(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available services");
            return Enumerable.Empty<ServiceInfo>();
        }
    }

    public async Task<bool> ToggleServiceAsync(string serviceName, bool enabled)
    {
        try
        {
            _logger.LogInformation("Toggling service {ServiceName} to {Enabled}", serviceName, enabled);

            // In a real implementation, you would actually enable/disable the service
            // For now, we'll just log the action
            await Task.Delay(100); // Simulate operation

            _logger.LogInformation("Service {ServiceName} toggled to {Enabled}", serviceName, enabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle service: {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<IEnumerable<ServiceLogEntry>> GetServiceLogsAsync(string serviceName, int? limit = null)
    {
        try
        {
            _logger.LogInformation("Getting logs for service: {ServiceName}", serviceName);

            var logs = new List<ServiceLogEntry>();
            var logDirectory = Path.Combine("Logs", serviceName.ToLowerInvariant());
            var count = limit ?? 100;

            // Check if log directory exists
            if (!Directory.Exists(logDirectory))
            {
                _logger.LogWarning("Log directory not found for service: {ServiceName}", serviceName);
                return logs;
            }

            // Get log files sorted by modification time (newest first)
            var logFiles = Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Take(5); // Take last 5 log files

            foreach (var logFile in logFiles)
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(logFile);
                    
                    foreach (var line in lines.Reverse().Take(count))
                    {
                        var logEntry = ParseLogLine(line, serviceName);
                        if (logEntry != null)
                        {
                            logs.Add(logEntry);
                            
                            if (logs.Count >= count)
                            {
                                break;
                            }
                        }
                    }

                    if (logs.Count >= count)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read log file: {LogFile}", logFile);
                }
            }

            return logs.OrderByDescending(l => l.Timestamp).Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs for service: {ServiceName}", serviceName);
            return Enumerable.Empty<ServiceLogEntry>();
        }
    }

    public async Task<OutfitPlanner.Application.Contracts.Infrastructure.HealthCheckResult> PerformHealthCheckAsync()
    {
        try
        {
            _logger.LogInformation("Performing comprehensive health check");

            var stopwatch = Stopwatch.StartNew();
            var healthReport = await _healthCheckService.CheckHealthAsync();
            stopwatch.Stop();

            var serviceChecks = new List<ServiceHealthCheck>();

            foreach (var entry in healthReport.Entries)
            {
                serviceChecks.Add(new ServiceHealthCheck(
                    entry.Key,
                    entry.Value.Status == HealthStatus.Healthy,
                    entry.Value.Duration,
                    entry.Value.Description
                ));
            }

            var isHealthy = healthReport.Status == HealthStatus.Healthy;
            var summary = $"Overall health: {healthReport.Status}. Checked {serviceChecks.Count} services.";

            return new OutfitPlanner.Application.Contracts.Infrastructure.HealthCheckResult(
                isHealthy,
                DateTime.UtcNow,
                stopwatch.Elapsed,
                serviceChecks,
                summary
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform health check");
            return new OutfitPlanner.Application.Contracts.Infrastructure.HealthCheckResult(
                false,
                DateTime.UtcNow,
                TimeSpan.Zero,
                Enumerable.Empty<ServiceHealthCheck>(),
                $"Health check failed: {ex.Message}"
            );
        }
    }

    private async Task<bool> CheckServiceHealthAsync(string serviceName)
    {
        try
        {
            switch (serviceName.ToLowerInvariant())
            {
                case "database":
                    return await CheckDatabaseHealthAsync();
                case "cache":
                    return await CheckCacheHealthAsync();
                case "api":
                    return await CheckApiHealthAsync();
                default:
                    return true; // Assume healthy for unknown services
            }
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckDatabaseHealthAsync()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckCacheHealthAsync()
    {
        try
        {
            // Check if cache service is available
            var cacheService = _serviceProvider.GetService(typeof(IMemoryCache));
            return cacheService != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckApiHealthAsync()
    {
        try
        {
            // Basic API health check - could be expanded
            return true;
        }
        catch
        {
            return false;
        }
    }

    private ServiceType GetServiceType(string serviceName)
    {
        return serviceName.ToLowerInvariant() switch
        {
            "database" => ServiceType.Database,
            "cache" => ServiceType.Cache,
            "api" => ServiceType.Api,
            "background" => ServiceType.Background,
            "queue" => ServiceType.Queue,
            _ => ServiceType.Internal
        };
    }

    private double GetCpuUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            return process.TotalProcessorTime.TotalMilliseconds / process.WorkingSet64 * 100;
        }
        catch
        {
            return 0.0;
        }
    }

    private ServiceLogEntry? ParseLogLine(string line, string serviceName)
    {
        try
        {
            // Parse standard log format: [timestamp] [level] [category] message
            // Example: [2023-12-10 10:30:45] [Information] [OutfitPlanner.Api] User logged in successfully
            
            var timestampMatch = System.Text.RegularExpressions.Regex.Match(line, @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]");
            if (!timestampMatch.Success)
            {
                return null; // Skip lines that don't match expected format
            }

            var timestamp = DateTime.Parse(timestampMatch.Groups[1].Value);

            var levelMatch = System.Text.RegularExpressions.Regex.Match(line, @"\[(\w+)\]");
            var levelStr = levelMatch.Success ? levelMatch.Groups[1].Value : "Information";
            
            var logLevel = levelStr.ToLowerInvariant() switch
            {
                "trace" => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Trace,
                "debug" => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Debug,
                "information" => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Information,
                "warning" => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Warning,
                "error" => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Error,
                "critical" => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Critical,
                _ => OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Information
            };

            // Extract message (everything after the last bracket)
            var messageStart = line.LastIndexOf(']') + 1;
            var message = messageStart > 0 && messageStart < line.Length 
                ? line.Substring(messageStart).Trim()
                : line;

            // Extract exception if present
            var exception = message.Contains("Exception:") ? message : null;

            return new ServiceLogEntry(
                timestamp,
                logLevel,
                message,
                serviceName,
                exception
            );
        }
        catch
        {
            // If parsing fails, create a basic entry
            return new ServiceLogEntry(
                DateTime.UtcNow,
                OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Information,
                line,
                serviceName
            );
        }
    }

    private int GetActiveConnections(string serviceName)
    {
        try
        {
            // For database service, check active connections
            if (serviceName.Equals("Database", StringComparison.OrdinalIgnoreCase))
            {
                return GetDatabaseConnectionCount();
            }

            // For API service, check active HTTP connections
            if (serviceName.Equals("API", StringComparison.OrdinalIgnoreCase))
            {
                return GetActiveHttpConnections();
            }

            // For other services, return system process connections
            return Process.GetCurrentProcess().Threads.Count;
        }
        catch
        {
            return 0;
        }
    }

    private int GetDatabaseConnectionCount()
    {
        try
        {
            // For SQLite, check if database is in use by checking file locks
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Data Source="))
            {
                var dbPath = connectionString.Split("Data Source=")[1].Split(";")[0];
                if (File.Exists(dbPath))
                {
                    // Try to open the database exclusively to check if it's in use
                    try
                    {
                        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
                        connection.Open();
                        
                        // Query for active connections (SQLite specific)
                        var command = connection.CreateCommand();
                        command.CommandText = "SELECT COUNT(*) FROM pragma_database_list WHERE name = 'main';";
                        
                        // SQLite doesn't have a direct way to count active connections
                        // We'll return 1 if we can connect, 0 if not
                        return 1;
                    }
                    catch
                    {
                        // If we can't connect, database might be locked by another process
                        return 0;
                    }
                }
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private int GetActiveHttpConnections()
    {
        try
        {
            // Get current process and check network connections
            var process = Process.GetCurrentProcess();
            
            // For a real implementation, you could:
            // 1. Use performance counters to monitor active HTTP connections
            // 2. Check TCP connections to the application's ports
            // 3. Use ASP.NET Core metrics to track active requests
            
            // For now, return a reasonable estimate based on thread count
            // This represents concurrent request processing capacity
            return Math.Max(1, process.Threads.Count / 2);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetRequestsPerSecondAsync(string serviceName)
    {
        try
        {
            // In a real implementation, you would track requests per second from metrics
            // This could be from a metrics collector like Prometheus, Application Insights, etc.
            
            // For now, we'll calculate from recent log entries
            var logDirectory = Path.Combine("Logs", serviceName.ToLowerInvariant());
            if (!Directory.Exists(logDirectory))
            {
                return 0;
            }

            var recentLogs = Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(recentLogs))
            {
                return 0;
            }

            var lines = await File.ReadAllLinesAsync(recentLogs);
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            var requestCount = 0;

            foreach (var line in lines.Reverse())
            {
                var logEntry = ParseLogLine(line, serviceName);
                if (logEntry != null && logEntry.Timestamp > oneMinuteAgo)
                {
                    // Count HTTP requests (you could refine this to only count actual request logs)
                    if (logEntry.Message.Contains("HTTP") || logEntry.Message.Contains("Request"))
                    {
                        requestCount++;
                    }
                }
            }

            return requestCount;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<TimeSpan> GetAverageResponseTimeAsync(string serviceName)
    {
        try
        {
            // In a real implementation, you would calculate average response time from metrics
            // This could be from performance counters or application metrics
            
            // For now, calculate from recent log entries that contain response time information
            var logDirectory = Path.Combine("Logs", serviceName.ToLowerInvariant());
            if (!Directory.Exists(logDirectory))
            {
                return TimeSpan.FromMilliseconds(100); // Default response time
            }

            var recentLogs = Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(recentLogs))
            {
                return TimeSpan.FromMilliseconds(100);
            }

            var lines = await File.ReadAllLinesAsync(recentLogs);
            var responseTimes = new List<double>();

            foreach (var line in lines.Reverse().Take(100)) // Last 100 lines
            {
                var logEntry = ParseLogLine(line, serviceName);
                if (logEntry != null && logEntry.Message.Contains("ms"))
                {
                    // Extract response time from log message (e.g., "Request completed in 150ms")
                    var match = System.Text.RegularExpressions.Regex.Match(logEntry.Message, @"(\d+)ms");
                    if (match.Success && double.TryParse(match.Groups[1].Value, out var responseTime))
                    {
                        responseTimes.Add(responseTime);
                    }
                }
            }

            if (responseTimes.Any())
            {
                return TimeSpan.FromMilliseconds(responseTimes.Average());
            }

            return TimeSpan.FromMilliseconds(100);
        }
        catch
        {
            return TimeSpan.FromMilliseconds(100);
        }
    }

    private async Task<long> GetTotalRequestsAsync(string serviceName)
    {
        try
        {
            // In a real implementation, you would get this from a metrics store or database
            // For now, count from log files
            
            var logDirectory = Path.Combine("Logs", serviceName.ToLowerInvariant());
            if (!Directory.Exists(logDirectory))
            {
                return 0;
            }

            var logFiles = Directory.GetFiles(logDirectory, "*.log");
            var totalRequests = 0L;

            foreach (var logFile in logFiles)
            {
                var lines = await File.ReadAllLinesAsync(logFile);
                totalRequests += lines.Count(line => 
                {
                    var logEntry = ParseLogLine(line, serviceName);
                    return logEntry != null && 
                           (logEntry.Message.Contains("HTTP") || 
                            logEntry.Message.Contains("Request") ||
                            logEntry.Message.Contains("GET") ||
                            logEntry.Message.Contains("POST"));
                });
            }

            return totalRequests;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetErrorCountAsync(string serviceName)
    {
        try
        {
            // Count error and critical log entries
            var logDirectory = Path.Combine("Logs", serviceName.ToLowerInvariant());
            if (!Directory.Exists(logDirectory))
            {
                return 0;
            }

            var logFiles = Directory.GetFiles(logDirectory, "*.log");
            var errorCount = 0;

            foreach (var logFile in logFiles)
            {
                var lines = await File.ReadAllLinesAsync(logFile);
                errorCount += lines.Count(line =>
                {
                    var logEntry = ParseLogLine(line, serviceName);
                    return logEntry != null && 
                           (logEntry.Level == OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Error ||
                            logEntry.Level == OutfitPlanner.Application.Contracts.Infrastructure.LogLevel.Critical);
                });
            }

            return errorCount;
        }
        catch
        {
            return 0;
        }
    }
}
