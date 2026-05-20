using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using OutfitPlanner.Application.Contracts.Infrastructure;
using System.Diagnostics;

namespace OutfitPlanner.Api.Middleware;

public class UserActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserActivityMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UserActivityMiddleware(RequestDelegate next, ILogger<UserActivityMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Only track if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await TrackUserActivity(context, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    private async Task TrackUserActivity(HttpContext context, long duration)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userActivityService = scope.ServiceProvider.GetRequiredService<IUserActivityService>();
            
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? context.User.FindFirst(ClaimTypes.Email)?.Value;
            
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
            {
                var request = context.Request;
                var path = request.Path;
                var method = request.Method;
                
                // Track page views for GET requests
                if (method == "GET" && !path.Value.StartsWith("/api/"))
                {
                    await userActivityService.RecordActivityAsync(userId, userName, 
                        "PageView", 
                        $"Visited {path}", 
                        GetClientIpAddress(context), 
                        GetUserAgent(context));
                }
                
                // Track API calls
                if (path.Value.StartsWith("/api/"))
                {
                    await userActivityService.RecordActivityAsync(userId, userName, 
                        "Other", 
                        $"API: {method} {path}", 
                        GetClientIpAddress(context), 
                        GetUserAgent(context));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track user activity");
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress.Split(',')[0].Trim();
        }
        
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent(HttpContext context)
    {
        return context.Request.Headers["User-Agent"].ToString();
    }
}
