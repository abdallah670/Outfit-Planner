using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.User;
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
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "Failed to update profile" });
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
}
