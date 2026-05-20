using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Api.Middleware;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var originalResponseBodyStream = context.Response.Body;
        
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Only audit if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await LogAuditEvent(context, stopwatch.ElapsedMilliseconds);
            }
            
            // Copy the response body back to the original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }
    }

    private async Task LogAuditEvent(HttpContext context, long duration)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<OutfitPlanner.Application.Common.Interfaces.Persistence.IUnitOfWork>();
            
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? context.User.FindFirst(ClaimTypes.Email)?.Value;
            
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
            {
                var request = context.Request;
                var response = context.Response;
                
                // Determine audit action based on HTTP method and path
                var auditAction = DetermineAuditAction(request.Method, request.Path);
                
                // Only log relevant audit events (not page views, etc.)
                if (ShouldLogAuditEvent(request.Method, request.Path, response.StatusCode))
                {
                    var auditLog = new OutfitPlanner.Domain.Entities.AuditLog
                    {
                        UserId = userId,
                        UserName = userName,
                        Action = auditAction,
                        EntityType = "Request",
                        EntityId = $"{request.Method} {request.Path}",
                        IpAddress = GetClientIpAddress(context),
                        Timestamp = DateTime.UtcNow,
                        OldValues = JsonSerializer.Serialize(new
                        {
                            RequestPath = request.Path,
                            RequestMethod = request.Method,
                            ResponseStatusCode = response.StatusCode,
                            Duration = duration,
                            QueryString = request.QueryString.ToString(),
                            RequestHeaders = GetRelevantHeaders(request),
                            UserRole = context.User.FindFirst(ClaimTypes.Role)?.Value
                        })
                    };
                    
                    await unitOfWork.AuditLogs.AddAsync(auditLog);
                    await unitOfWork.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event");
        }
    }

    private string DetermineAuditAction(string method, string path)
    {
        // Admin actions
        if (path.StartsWith("/api/admin"))
        {
            return method switch
            {
                "POST" => "Admin_Create",
                "PUT" => "Admin_Update", 
                "DELETE" => "Admin_Delete",
                "GET" => "Admin_View",
                _ => "Admin_Access"
            };
        }
        
        // User management actions
        if (path.Contains("/auth/") || path.Contains("/user/"))
        {
            return method switch
            {
                "POST" => "User_Create",
                "PUT" => "User_Update",
                "DELETE" => "User_Delete", 
                _ => "User_Access"
            };
        }
        
        // Content management actions
        if (path.Contains("/outfit") || path.Contains("/clothing") || path.Contains("/post"))
        {
            return method switch
            {
                "POST" => "Content_Create",
                "PUT" => "Content_Update",
                "DELETE" => "Content_Delete",
                _ => "Content_Access"
            };
        }
        
        // Default action
        return method switch
        {
            "GET" => "Read",
            "POST" => "Create",
            "PUT" => "Update",
            "DELETE" => "Delete",
            "PATCH" => "Update",
            _ => "Access"
        };
    }

    private bool ShouldLogAuditEvent(string method, string path, int statusCode)
    {
        // Don't log successful GET requests for static content or page views
        if (method == "GET" && statusCode == 200 && !path.StartsWith("/api/"))
        {
            return false;
        }
        
        // Don't log health checks or swagger
        if (path.StartsWith("/health") || path.StartsWith("/swagger") || path.StartsWith("/hangfire"))
        {
            return false;
        }
        
        // Log all API calls, failed requests, and non-GET requests
        return path.StartsWith("/api/") || 
               statusCode >= 400 || 
               method != "GET";
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

    private Dictionary<string, string> GetRelevantHeaders(HttpRequest request)
    {
        var headers = new Dictionary<string, string>();
        
        // Only include relevant headers for audit purposes
        var relevantHeaders = new[] { "Content-Type", "Accept", "Authorization", "X-Requested-With" };
        
        foreach (var header in relevantHeaders)
        {
            if (request.Headers.ContainsKey(header))
            {
                headers[header] = request.Headers[header].ToString();
            }
        }
        
        return headers;
    }
}
