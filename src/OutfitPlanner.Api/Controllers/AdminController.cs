using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Common;
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
            return StatusCode(500, "Internal server error");
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
            _logger.LogError(ex, "Failed to get user detail");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users/{userId}/stats")]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics(string userId)
    {
        try
        {
            var result = await _mediator.Send(new GetUserStatisticsQuery(userId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("users/{userId}/ban")]
    public async Task<ActionResult<Result>> BanUser(string userId, [FromBody] BanUserRequest request)
    {
        try
        {
            var result = await _mediator.Send(new BanUserCommand(userId, request.Reason, request.Expiry?.UtcDateTime));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ban user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("users/{userId}/unban")]
    public async Task<ActionResult<Result>> UnbanUser(string userId)
    {
        try
        {
            var result = await _mediator.Send(new UnbanUserCommand(userId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unban user");
            return StatusCode(500, "Internal server error");
        }
    }

    // Post Management
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
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("content/posts/{postId}")]
    public async Task<ActionResult<Result>> DeletePost(Guid postId)
    {
        try
        {
            var result = await _mediator.Send(new DeletePostCommand(postId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete post");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("content/posts/bulk")]
    public async Task<ActionResult<BulkOperationResponse>> BulkPostOperation([FromBody] BulkPostOperationRequest request)
    {
        try
        {
            var operations = request.Operations.Select(o => new PostOperation(o.PostId, o.Type, o.Reason)).ToList();
            var result = await _mediator.Send(new BulkPostOperationCommand(operations));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk post operation");
            return StatusCode(500, "Internal server error");
        }
    }

    // Report Management
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
            return StatusCode(500, "Internal server error");
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
            _logger.LogError(ex, "Failed to get report detail");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reports/{reportId}/resolve")]
    public async Task<ActionResult<Result>> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ResolveReportCommand(reportId, request.Resolution, request.TakeAction));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve report");
            return StatusCode(500, "Internal server error");
        }
    }

    // Poll Management
    [HttpGet("content/polls")]
    public async Task<ActionResult<PaginatedResult<AdminPostDto>>> GetPolls([FromQuery] ContentFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetPostsQuery(filter with { ContentType = "Poll" }));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get polls");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("content/polls/{pollId}/close")]
    public async Task<ActionResult<Result>> ClosePoll(Guid pollId, [FromBody] string reason = "Closed by admin")
    {
        try
        {
            var result = await _mediator.Send(new ClosePollCommand(pollId, reason));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close poll");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("content/polls/{pollId}")]
    public async Task<ActionResult<Result>> DeletePoll(Guid pollId)
    {
        try
        {
            var result = await _mediator.Send(new DeletePollCommand(pollId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete poll");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("content/polls/bulk")]
    public async Task<ActionResult<BulkOperationResponse>> BulkPollOperation([FromBody] BulkPollOperationRequest request)
    {
        try
        {
            var operations = request.Operations.Select(o => new PollOperation(o.PostId, o.Type, o.Reason)).ToList();
            var result = await _mediator.Send(new BulkPollOperationCommand(operations));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk poll operation");
            return StatusCode(500, "Internal server error");
        }
    }

    // Outfit Management
    [HttpGet("content/outfits")]
    public async Task<ActionResult<PaginatedResult<AdminPostDto>>> GetOutfits([FromQuery] ContentFilterRequest filter)
    {
        try
        {
            var result = await _mediator.Send(new GetPostsQuery(filter with { ContentType = "Outfit" }));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get outfits");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("content/outfits/{outfitId}")]
    public async Task<ActionResult<Result>> DeleteOutfit(Guid outfitId)
    {
        try
        {
            var result = await _mediator.Send(new DeleteOutfitCommand(outfitId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete outfit");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("content/outfits/bulk")]
    public async Task<ActionResult<BulkOperationResponse>> BulkOutfitOperation([FromBody] BulkOutfitOperationRequest request)
    {
        try
        {
            var operations = request.Operations.Select(o => new OutfitOperation(o.PostId, o.Type, o.Reason)).ToList();
            var result = await _mediator.Send(new BulkOutfitOperationCommand(operations));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk outfit operation");
            return StatusCode(500, "Internal server error");
        }
    }

    // Analytics
    [HttpGet("analytics/realtime")]
    public async Task<ActionResult<OutfitPlanner.Application.DTOs.Admin.RealtimeAnalyticsDto>> GetRealtimeAnalytics()
    {
        try
        {
            var result = await _mediator.Send(new GetRealtimeAnalyticsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get realtime analytics");
            return StatusCode(500, "Internal server error");
        }
    }

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
            return StatusCode(500, "Internal server error");
        }
    }

    // System Settings
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
            _logger.LogError(ex, "Failed to get system settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("settings/maintenance")]
    public async Task<ActionResult<Result>> SetMaintenanceMode([FromBody] SetMaintenanceModeCommand request)
    {
        try
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set maintenance mode");
            return StatusCode(500, "Internal server error");
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
            return StatusCode(500, "Internal server error");
        }
    }

    // System Operations
    [HttpPost("system/backup")]
    public async Task<ActionResult<BackupResult>> CreateBackup([FromBody] CreateBackupRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CreateBackupCommand(request));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("system/restart")]
    public async Task<ActionResult<Result>> RestartService([FromBody] string serviceName)
    {
        try
        {
            var result = await _mediator.Send(new RestartServiceCommand(serviceName));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart service");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("system/cache/clear")]
    public async Task<ActionResult<Result>> ClearCache([FromBody] string? cacheKey)
    {
        try
        {
            var result = await _mediator.Send(new ClearCacheCommand(cacheKey));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache");
            return StatusCode(500, "Internal server error");
        }
    }
}
