using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Contracts.Identity;
using OutfitPlanner.Application.Models.Identity;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJWTService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJWTService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        try
        {
            return Ok(await _authenticationService.Login(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for user {Email}", request.Email);
            if (ex.Message.Contains("not found") || ex.Message.Contains("not valid"))
            {
                return BadRequest(new { message = ex.Message });
            }
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        try 
        {
            return Ok(await _authenticationService.Register(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration for user {UserName}", request.UserName);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(string token, string refreshToken)
    {
        return Ok(await _authenticationService.RefreshToken(token, refreshToken));
    }
}
