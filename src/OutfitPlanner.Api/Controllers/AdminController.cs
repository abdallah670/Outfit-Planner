using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Infrastructure.Services;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AccountUnlockBackgroundJob _accountUnlockJob;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, AccountUnlockBackgroundJob accountUnlockJob, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _accountUnlockJob = accountUnlockJob;
        _logger = logger;
    }

    /// <summary>
    /// Get all currently locked out user accounts
    /// </summary>
    [HttpGet("locked-accounts")]
    public async Task<ActionResult<List<LockedAccountDto>>> GetLockedOutAccounts()
    {
        try
        {
            var lockedAccounts = await _mediator.Send(new GetLockedAccountsQuery());
            return Ok(lockedAccounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get locked out accounts");
            return StatusCode(500, new { message = "Failed to retrieve locked accounts" });
        }
    }

    // User Management
    [HttpGet("users")]
    public async Task<ActionResult<PaginatedResult<AdminUserDto>>> GetUsers([FromQuery] UserFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetUsersQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get users");
            return StatusCode(500, new { message = "Failed to retrieve users" });
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<AdminUserDetailDto>> GetUserDetail(string userId)
    {
        try
        {
            var result = await _mediator.Send(new GetUserDetailQuery(userId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user detail for {UserId}", userId);
            return StatusCode(500, new { message = "Failed to retrieve user details" });
        }
    }

    [HttpPost("users/{userId}/ban")]
    public async Task<ActionResult> BanUser(string userId, [FromBody] BanUserRequest request)
    {
        try
        {
            var result = await _mediator.Send(new BanUserCommand(userId, request.Reason, request.Expiry));
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ban user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to ban user" });
        }
    }

    [HttpPost("users/{userId}/unban")]
    public async Task<ActionResult> UnbanUser(string userId)
    {
        try
        {
            var result = await _mediator.Send(new UnbanUserCommand(userId));
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unban user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to unban user" });
        }
    }

    // Reports
    [HttpGet("reports")]
    public async Task<ActionResult<PaginatedResult<ContentReportDto>>> GetReports([FromQuery] ReportFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetReportsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get reports");
            return StatusCode(500, new { message = "Failed to retrieve reports" });
        }
    }

    [HttpGet("reports/{reportId}")]
    public async Task<ActionResult<ContentReportDetailDto>> GetReportDetail(Guid reportId)
    {
        try
        {
            var result = await _mediator.Send(new GetReportDetailQuery(reportId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get report detail for {ReportId}", reportId);
            return StatusCode(500, new { message = "Failed to retrieve report details" });
        }
    }

    [HttpPost("reports/{reportId}/resolve")]
    public async Task<ActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ResolveReportCommand(reportId, request.Resolution, request.TakeAction));
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve report {ReportId}", reportId);
            return StatusCode(500, new { message = "Failed to resolve report" });
        }
    }

    // Settings
    [HttpGet("settings")]
    public async Task<ActionResult<List<SystemSettingDto>>> GetSettings()
    {
        try
        {
            var result = await _mediator.Send(new GetAllSettingsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings");
            return StatusCode(500, new { message = "Failed to retrieve settings" });
        }
    }

    [HttpPut("settings/{key}")]
    public async Task<ActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateSettingCommand(key, request.Value));
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update setting {Key}", key);
            return StatusCode(500, new { message = "Failed to update setting" });
        }
    }

    // Analytics
    [HttpGet("analytics/dashboard")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboardAnalytics(
        [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        try
        {
            var result = await _mediator.Send(new GetDashboardAnalyticsQuery(startDate, endDate));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard analytics");
            return StatusCode(500, new { message = "Failed to retrieve analytics" });
        }
    }

    // Audit Logs
    [HttpGet("audit-logs")]
    public async Task<ActionResult<PaginatedResult<AuditLogDto>>> GetAuditLogs([FromQuery] AuditLogFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetAuditLogsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs");
            return StatusCode(500, new { message = "Failed to retrieve audit logs" });
        }
    }

    // Account Unlock (kept from Phase 1)
    [HttpPost("unlock-account/{userId}")]
    public async Task<ActionResult> UnlockAccount(string userId)
    {
        try
        {
            var result = await _accountUnlockJob.ManualUnlockAccount(userId);
            return result.IsSuccess ? Ok(new { message = "Account unlocked successfully" }) : BadRequest(new { message = "Failed to unlock account" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlock account for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to unlock account" });
        }
    }

    // Content Management - Posts
    [HttpGet("content/posts")]
    public async Task<ActionResult<PaginatedResult<AdminPostDto>>> GetPosts([FromQuery] ContentFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetPostsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get posts");
            return StatusCode(500, new { message = "Failed to retrieve posts" });
        }
    }

    [HttpPost("content/posts/{postId}/approve")]
    public async Task<ActionResult> ApprovePost(Guid postId)
    {
        try
        {
            var result = await _mediator.Send(new ApprovePostCommand(postId));
            return result.IsSuccess ? Ok(new { message = "Post approved successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve post {PostId}", postId);
            return StatusCode(500, new { message = "Failed to approve post" });
        }
    }

    [HttpPost("content/posts/{postId}/reject")]
    public async Task<ActionResult> RejectPost(Guid postId, [FromBody] RejectPostRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RejectPostCommand(postId, request.Reason));
            return result.IsSuccess ? Ok(new { message = "Post rejected successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject post {PostId}", postId);
            return StatusCode(500, new { message = "Failed to reject post" });
        }
    }

    [HttpDelete("content/posts/{postId}")]
    public async Task<ActionResult> DeletePost(Guid postId)
    {
        try
        {
            var result = await _mediator.Send(new DeletePostCommand(postId));
            return result.IsSuccess ? Ok(new { message = "Post deleted successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete post {PostId}", postId);
            return StatusCode(500, new { message = "Failed to delete post" });
        }
    }

    [HttpPost("content/posts/bulk")]
    public async Task<ActionResult<BulkOperationResponse>> BulkPostOperations([FromBody] BulkPostOperationRequest request)
    {
        try
        {
            var operations = request.Operations.Select(o => new PostOperation(o.PostId, o.Type, o.Reason)).ToList();
            var result = await _mediator.Send(new BulkPostOperationCommand(operations));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk post operations");
            return StatusCode(500, new { message = "Bulk operation failed" });
        }
    }

    // Content Management - Polls
    [HttpGet("content/polls")]
    public async Task<ActionResult<PaginatedResult<AdminPollDto>>> GetPolls([FromQuery] ContentFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetPollsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get polls");
            return StatusCode(500, new { message = "Failed to retrieve polls" });
        }
    }

    [HttpPost("content/polls/{pollId}/close")]
    public async Task<ActionResult> ClosePoll(Guid pollId, [FromBody] ClosePollRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ClosePollCommand(pollId, request.Reason));
            return result.IsSuccess ? Ok(new { message = "Poll closed successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close poll {PollId}", pollId);
            return StatusCode(500, new { message = "Failed to close poll" });
        }
    }

    [HttpPost("content/polls/{pollId}/feature")]
    public async Task<ActionResult> FeaturePoll(Guid pollId)
    {
        try
        {
            var result = await _mediator.Send(new FeaturePollCommand(pollId));
            return result.IsSuccess ? Ok(new { message = "Poll featured successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to feature poll {PollId}", pollId);
            return StatusCode(500, new { message = "Failed to feature poll" });
        }
    }

    [HttpPost("content/polls/{pollId}/unfeature")]
    public async Task<ActionResult> UnfeaturePoll(Guid pollId)
    {
        try
        {
            var result = await _mediator.Send(new UnfeaturePollCommand(pollId));
            return result.IsSuccess ? Ok(new { message = "Poll unfeatured successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unfeature poll {PollId}", pollId);
            return StatusCode(500, new { message = "Failed to unfeature poll" });
        }
    }

    [HttpDelete("content/polls/{pollId}")]
    public async Task<ActionResult> DeletePoll(Guid pollId)
    {
        try
        {
            var result = await _mediator.Send(new DeletePollCommand(pollId));
            return result.IsSuccess ? Ok(new { message = "Poll deleted successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete poll {PollId}", pollId);
            return StatusCode(500, new { message = "Failed to delete poll" });
        }
    }

    [HttpPost("content/polls/bulk")]
    public async Task<ActionResult<BulkOperationResponse>> BulkPollOperations([FromBody] BulkPollOperationRequest request)
    {
        try
        {
            var operations = request.Operations.Select(o => new PollOperation(o.PollId, o.Type, o.Reason)).ToList();
            var result = await _mediator.Send(new BulkPollOperationCommand(operations));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk poll operations");
            return StatusCode(500, new { message = "Bulk operation failed" });
        }
    }

    // Content Management - Outfits
    [HttpGet("content/outfits")]
    public async Task<ActionResult<PaginatedResult<AdminOutfitDto>>> GetOutfits([FromQuery] ContentFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetOutfitsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get outfits");
            return StatusCode(500, new { message = "Failed to retrieve outfits" });
        }
    }

    [HttpPost("content/outfits/{outfitId}/feature")]
    public async Task<ActionResult> FeatureOutfit(Guid outfitId)
    {
        try
        {
            var result = await _mediator.Send(new FeatureOutfitCommand(outfitId));
            return result.IsSuccess ? Ok(new { message = "Outfit featured successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to feature outfit {OutfitId}", outfitId);
            return StatusCode(500, new { message = "Failed to feature outfit" });
        }
    }

    [HttpPost("content/outfits/{outfitId}/unfeature")]
    public async Task<ActionResult> UnfeatureOutfit(Guid outfitId)
    {
        try
        {
            var result = await _mediator.Send(new UnfeatureOutfitCommand(outfitId));
            return result.IsSuccess ? Ok(new { message = "Outfit unfeatured successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unfeature outfit {OutfitId}", outfitId);
            return StatusCode(500, new { message = "Failed to unfeature outfit" });
        }
    }

    [HttpPost("content/outfits/{outfitId}/approve")]
    public async Task<ActionResult> ApproveOutfit(Guid outfitId)
    {
        try
        {
            var result = await _mediator.Send(new ApproveOutfitCommand(outfitId));
            return result.IsSuccess ? Ok(new { message = "Outfit approved successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve outfit {OutfitId}", outfitId);
            return StatusCode(500, new { message = "Failed to approve outfit" });
        }
    }

    [HttpPost("content/outfits/{outfitId}/reject")]
    public async Task<ActionResult> RejectOutfit(Guid outfitId, [FromBody] RejectOutfitRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RejectOutfitCommand(outfitId, request.Reason));
            return result.IsSuccess ? Ok(new { message = "Outfit rejected successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject outfit {OutfitId}", outfitId);
            return StatusCode(500, new { message = "Failed to reject outfit" });
        }
    }

    [HttpDelete("content/outfits/{outfitId}")]
    public async Task<ActionResult> DeleteOutfit(Guid outfitId)
    {
        try
        {
            var result = await _mediator.Send(new DeleteOutfitCommand(outfitId));
            return result.IsSuccess ? Ok(new { message = "Outfit deleted successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete outfit {OutfitId}", outfitId);
            return StatusCode(500, new { message = "Failed to delete outfit" });
        }
    }

    [HttpPost("content/outfits/bulk")]
    public async Task<ActionResult<BulkOperationResponse>> BulkOutfitOperations([FromBody] BulkOutfitOperationRequest request)
    {
        try
        {
            var operations = request.Operations.Select(o => new OutfitOperation(o.OutfitId, o.Type, o.Reason)).ToList();
            var result = await _mediator.Send(new BulkOutfitOperationCommand(operations));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk outfit operations");
            return StatusCode(500, new { message = "Bulk operation failed" });
        }
    }

    // Analytics Management
    [HttpGet("analytics/detailed")]
    public async Task<ActionResult<DetailedAnalyticsDto>> GetDetailedAnalytics([FromQuery] AnalyticsFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetDetailedAnalyticsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get detailed analytics");
            return StatusCode(500, new { message = "Failed to retrieve analytics" });
        }
    }

    [HttpGet("analytics/realtime")]
    public async Task<ActionResult<RealtimeAnalyticsDto>> GetRealtimeAnalytics()
    {
        try
        {
            var result = await _mediator.Send(new GetRealtimeAnalyticsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get realtime analytics");
            return StatusCode(500, new { message = "Failed to retrieve realtime analytics" });
        }
    }

    [HttpPost("analytics/export")]
    public async Task<ActionResult<AnalyticsExportResult>> ExportAnalytics([FromBody] ExportAnalyticsRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ExportAnalyticsCommand(request.Format, request.StartDate, request.EndDate));
            
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{result.FileName}\"");
            return File(result.FileContents, result.ContentType, result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export analytics");
            return StatusCode(500, new { message = "Failed to export analytics" });
        }
    }

    // System Management
    [HttpGet("system/health")]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
    {
        try
        {
            var result = await _mediator.Send(new GetSystemHealthQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system health");
            return StatusCode(500, new { message = "Failed to retrieve system health" });
        }
    }

    [HttpGet("system/logs")]
    public async Task<ActionResult<PaginatedResult<SystemLogDto>>> GetSystemLogs([FromQuery] SystemLogFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetSystemLogsQuery(filter));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system logs");
            return StatusCode(500, new { message = "Failed to retrieve system logs" });
        }
    }

    [HttpGet("system/performance")]
    public async Task<ActionResult<SystemPerformanceDto>> GetSystemPerformance()
    {
        try
        {
            var result = await _mediator.Send(new GetSystemPerformanceQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system performance");
            return StatusCode(500, new { message = "Failed to retrieve system performance" });
        }
    }

    [HttpPost("system/maintenance")]
    public async Task<ActionResult> SetMaintenanceMode([FromBody] SetMaintenanceModeRequest request)
    {
        try
        {
            var result = await _mediator.Send(new SetMaintenanceModeCommand(request.Enabled, request.Message));
            return result.IsSuccess ? Ok(new { message = "Maintenance mode updated successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set maintenance mode");
            return StatusCode(500, new { message = "Failed to update maintenance mode" });
        }
    }

    [HttpPost("system/backup")]
    public async Task<ActionResult<BackupResult>> CreateBackup([FromBody] BackupRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CreateBackupCommand(request));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup");
            return StatusCode(500, new { message = "Failed to create backup" });
        }
    }

    [HttpPost("system/restart/{serviceName}")]
    public async Task<ActionResult> RestartService(string serviceName)
    {
        try
        {
            var result = await _mediator.Send(new RestartServiceCommand(serviceName));
            return result.IsSuccess ? Ok(new { message = "Service restarted successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart service {ServiceName}", serviceName);
            return StatusCode(500, new { message = "Failed to restart service" });
        }
    }

    [HttpPost("system/clear-cache")]
    public async Task<ActionResult> ClearCache([FromBody] ClearCacheRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ClearCacheCommand(request.CacheKey));
            return result.IsSuccess ? Ok(new { message = "Cache cleared successfully" }) : BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache");
            return StatusCode(500, new { message = "Failed to clear cache" });
        }
    }

    /// <summary>
    /// Admin-only test endpoint to verify admin access
    /// </summary>
    [HttpGet("test")]
    public IActionResult AdminTest()
    {
        return Ok(new { 
            message = "You have admin access!",
            timestamp = DateTime.UtcNow,
            user = User.Identity?.Name 
        });
    }
}
