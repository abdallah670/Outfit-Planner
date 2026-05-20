using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;
using System.Net;
using System.Web;

namespace OutfitPlanner.Api.Middleware;

public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceMiddleware> _logger;

    public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMaintenanceService maintenanceService)
    {
        try
        {
            var maintenanceStatus = await maintenanceService.GetMaintenanceStatusAsync();
            var configuration = await maintenanceService.GetMaintenanceConfigurationAsync();

            // Check if maintenance mode is active
            if (!maintenanceStatus.IsEnabled)
            {
                await _next(context);
                return;
            }

            // Check if request should bypass maintenance mode
            if (ShouldBypassMaintenance(context, configuration))
            {
                await _next(context);
                return;
            }

            // Return maintenance response
            await ReturnMaintenanceResponse(context, maintenanceStatus, configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in maintenance middleware");
            // If there's an error, allow the request to proceed
            await _next(context);
        }
    }

    private static bool ShouldBypassMaintenance(HttpContext context, MaintenanceConfiguration configuration)
    {
        var requestPath = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var clientIp = GetClientIpAddress(context);

        // Check if IP is in bypass list
        if (configuration.BypassIps.Contains(clientIp))
        {
            return true;
        }

        // Check if path is in allowed paths
        if (configuration.AllowedPaths.Any(allowedPath => 
            requestPath.StartsWith(allowedPath.ToLowerInvariant())))
        {
            return true;
        }

        // Check if user is admin (if admin access is allowed)
        if (configuration.AllowAdminAccess && context.User.IsInRole("Admin"))
        {
            return true;
        }

        return false;
    }

    private static async Task ReturnMaintenanceResponse(HttpContext context, 
        MaintenanceStatus status, MaintenanceConfiguration configuration)
    {
        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
        context.Response.ContentType = "text/html; charset=utf-8";

        var html = GenerateMaintenancePage(status, configuration);
        await context.Response.WriteAsync(html);
    }

    private static string GenerateMaintenancePage(MaintenanceStatus status, MaintenanceConfiguration configuration)
    {
        var message = status.Message ?? "System is currently under maintenance. Please try again later.";
        
        var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>System Maintenance</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            margin: 0;
            padding: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .maintenance-container {{
            background: white;
            padding: 3rem;
            border-radius: 12px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            text-align: center;
            max-width: 500px;
            margin: 20px;
        }}
        .maintenance-icon {{
            font-size: 4rem;
            margin-bottom: 1rem;
        }}
        .maintenance-title {{
            color: #333;
            font-size: 2rem;
            margin-bottom: 1rem;
            font-weight: 600;
        }}
        .maintenance-message {{
            color: #666;
            font-size: 1.1rem;
            line-height: 1.6;
            margin-bottom: 2rem;
        }}
        .maintenance-time {{
            color: #999;
            font-size: 0.9rem;
            margin-top: 1rem;
        }}
        .refresh-button {{
            background: #667eea;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 6px;
            font-size: 1rem;
            cursor: pointer;
            transition: background 0.3s;
        }}
        .refresh-button:hover {{
            background: #5a6fd8;
        }}
    </style>
</head>
<body>
    <div class='maintenance-container'>
        <div class='maintenance-icon'>🔧</div>
        <h1 class='maintenance-title'>Under Maintenance</h1>
        <p class='maintenance-message'>{WebUtility.HtmlEncode(message)}</p>
        <button class='refresh-button' onclick='window.location.reload()'>Refresh</button>
        <div class='maintenance-time'>
            Maintenance started at {status.EnabledAt:yyyy-MM-dd HH:mm:ss}
        </div>
    </div>
    <script>
        // Auto-refresh every 5 minutes
        setTimeout(() => window.location.reload(), 300000);
    </script>
</body>
</html>";

        return html;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        
        // Check for X-Forwarded-For header (when behind proxy/load balancer)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ipAddress = forwardedFor.FirstOrDefault()?.Split(',')[0]?.Trim();
        }
        
        return ipAddress ?? "unknown";
    }
}
