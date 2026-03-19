using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var query = new GetUserProfileQuery { UserId = userId };
            var profile = await _mediator.Send(query);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "Failed to retrieve profile" });
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Log the request for debugging (excluding PII)
            _logger.LogInformation("Updating profile for user {UserId}. HasPreferences: {HasPreferences}, HasStyleProfile: {HasStyleProfile}", 
                userId, 
                request.Preferences != null, 
                request.StyleProfile != null);

            var command = new UpdateUserProfileCommand 
            { 
                UserId = userId, 
                Request = request 
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            _logger.LogWarning("Profile update failed for user {UserId}: {Message}", userId, response.Message);
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "Failed to update profile", error = ex.Message });
        }
    }
    /// <summary>
    /// Upload profile picture
    /// </summary>
    [HttpGet("profile-picture")]
    public async Task<IActionResult> GetProfilePicture()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var query = new GetProfilePictureQuery { UserId = userId };
            var profilePicture = await _mediator.Send(query);
            return Ok(profilePicture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile picture");
            return StatusCode(500, new { message = "Failed to retrieve profile picture" });
        }
    }

    
       
    
    /// <summary>
    /// Upload profile picture
    /// </summary>
    [HttpPost("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new UploadProfilePictureCommand 
            { 
                UserId = userId, 
                File = file 
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture");
            return StatusCode(500, new { message = "Failed to upload profile picture" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new ChangePasswordCommand 
            { 
                UserId = userId, 
                Request = request 
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "Failed to change password" });
        }
    }

    /// <summary>
    /// Update user email
    /// </summary>
    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new UpdateEmailCommand
            {
                UserId = userId,
                Request = request
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email");
            return StatusCode(500, new { message = "Failed to update email" });
        }
    }

    #region Style Rules

    /// <summary>
    /// Get all style rules for current user
    /// </summary>
    [HttpGet("style-rules")]
    public async Task<ActionResult<List<StyleRuleDto>>> GetStyleRules()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var query = new GetStyleRulesQuery { UserId = userId };
            var rules = await _mediator.Send(query);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving style rules");
            return StatusCode(500, new { message = "Failed to retrieve style rules" });
        }
    }

    /// <summary>
    /// Create a new style rule
    /// </summary>
    [HttpPost("style-rules")]
    public async Task<ActionResult<StyleRuleDto>> CreateStyleRule([FromBody] CreateStyleRuleDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new CreateStyleRuleCommand 
            { 
                UserId = userId, 
                Rule = request 
            };
            var createdRule = await _mediator.Send(command);
            return Ok(createdRule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating style rule");
            return StatusCode(500, new { message = "Failed to create style rule" });
        }
    }

    /// <summary>
    /// Update a style rule
    /// </summary>
    [HttpPut("style-rules/{id}")]
    public async Task<IActionResult> UpdateStyleRule(Guid id, [FromBody] UpdateStyleRuleDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new UpdateStyleRuleCommand 
            { 
                UserId = userId,
                RuleId = id,
                Rule = request 
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "Style rule not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating style rule");
            return StatusCode(500, new { message = "Failed to update style rule" });
        }
    }

    /// <summary>
    /// Delete a style rule
    /// </summary>
    [HttpDelete("style-rules/{id}")]
    public async Task<IActionResult> DeleteStyleRule(Guid id)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new DeleteStyleRuleCommand 
            { 
                UserId = userId,
                RuleId = id
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "Style rule not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting style rule");
            return StatusCode(500, new { message = "Failed to delete style rule" });
        }
    }

    #endregion

    #region App Preferences

    /// <summary>
    /// Get app preferences for current user
    /// </summary>
    [HttpGet("app-preferences")]
    public async Task<ActionResult<AppPreferencesDto>> GetAppPreferences()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var query = new GetAppPreferencesQuery { UserId = userId };
            var preferences = await _mediator.Send(query);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving app preferences");
            return StatusCode(500, new { message = "Failed to retrieve app preferences" });
        }
    }

    /// <summary>
    /// Update app preferences for current user
    /// </summary>
    [HttpPut("app-preferences")]
    public async Task<IActionResult> UpdateAppPreferences([FromBody] UpdateAppPreferencesDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var command = new UpdateAppPreferencesCommand { UserId = userId, Preferences = request };
            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating app preferences");
            return StatusCode(500, new { message = "Failed to update app preferences" });
        }
    }

    #endregion

    #region Notification Settings

    /// <summary>
    /// Get notification settings for current user
    /// </summary>
    [HttpGet("notification-settings")]
    public async Task<ActionResult<NotificationSettingsDto>> GetNotificationSettings()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var query = new GetNotificationSettingsQuery { UserId = userId };
            var settings = await _mediator.Send(query);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification settings");
            return StatusCode(500, new { message = "Failed to retrieve notification settings" });
        }
    }

    /// <summary>
    /// Update notification settings for current user
    /// </summary>
    [HttpPut("notification-settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var command = new UpdateNotificationSettingsCommand { UserId = userId, Settings = request };
            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification settings");
            return StatusCode(500, new { message = "Failed to update notification settings" });
        }
    }

    #endregion

    #region Connected Accounts

    /// <summary>
    /// Get connected social accounts
    /// </summary>
    [HttpGet("connected-accounts")]
    public async Task<ActionResult<List<ConnectedAccountDto>>> GetConnectedAccounts()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var query = new GetConnectedAccountsQuery { UserId = userId };
            var accounts = await _mediator.Send(query);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connected accounts");
            return StatusCode(500, new { message = "Failed to retrieve connected accounts" });
        }
    }

    /// <summary>
    /// Initiate connection of an external account (Google/Facebook)
    /// Returns the OAuth authorization URL
    /// </summary>
    [HttpPost("connected-accounts/connect")]
    public async Task<ActionResult<ConnectAccountResponseDto>> ConnectAccount([FromBody] ConnectAccountRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Return the OAuth URL for the requested provider
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            string authorizationUrl = request.Provider.ToLower() switch
            {
                "google" => $"{baseUrl}/api/Auth/google?userId={userId}&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}",
                "facebook" => $"{baseUrl}/api/Auth/facebook?userId={userId}&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}",
                _ => throw new ArgumentException("Unsupported provider")
            };

            return Ok(new ConnectAccountResponseDto { AuthorizationUrl = authorizationUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating account connection");
            return StatusCode(500, new { message = "Failed to initiate account connection" });
        }
    }

    /// <summary>
    /// Disconnect an external account
    /// </summary>
    [HttpPost("connected-accounts/disconnect")]
    public async Task<IActionResult> DisconnectAccount([FromBody] DisconnectAccountRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var command = new DisconnectAccountCommand 
            { 
                UserId = userId, 
                Provider = request.Provider 
            };
            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting account");
            return StatusCode(500, new { message = "Failed to disconnect account" });
        }
    }

    #endregion


    #region Account Management

    /// <summary>
    /// Delete user account
    /// </summary>
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var command = new DeleteAccountCommand { UserId = userId };
            var response = await _mediator.Send(command);
            
            if (response.Success)
                return Ok(response);
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account");
            return StatusCode(500, new { message = "Failed to delete account" });
        }
    }

    /// <summary>
    /// Export user data as CSV
    /// </summary>
    [HttpGet("export-data")]
    public async Task<IActionResult> ExportData()
    {
        try
        {
            var userId = User.FindFirst(OutfitPlanner.Application.Constants.CustomClaimTypes.Uid)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var query = new ExportUserDataQuery { UserId = userId };
            var exportData = await _mediator.Send(query);
            
            if (exportData.Data == null || exportData.Data.Length == 0)
                return BadRequest(new { message = "No data to export" });
            
            return File(exportData.Data, exportData.ContentType, exportData.FileName);
        }
        catch (OutfitPlanner.Application.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found while exporting data");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user data");
            return StatusCode(500, new { message = "Failed to export user data" });
        }
    }

    #endregion
}
