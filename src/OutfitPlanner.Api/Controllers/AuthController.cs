using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Contracts.Identity;
using OutfitPlanner.Application.Models.Identity;
using System.Security.Claims;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJWTService _authenticationService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(IJWTService authenticationService, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _authenticationService = authenticationService;
        _logger = logger;
        _configuration = configuration;
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

    public record RefreshTokenRequest(string Token, string RefreshToken);

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            return Ok(await _authenticationService.RefreshToken(request.Token, request.RefreshToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token refresh");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    #region Social Login

    /// <summary>
    /// Initiate Google OAuth login
    /// </summary>
    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(ExternalCallback), new { provider = "Google" }),
            Items = { { "LoginProvider", "Google" } }
        };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Initiate Facebook OAuth login
    /// </summary>
    [HttpGet("facebook")]
    public IActionResult FacebookLogin()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(ExternalCallback), new { provider = "Facebook" }),
            Items = { { "LoginProvider", "Facebook" } }
        };
        return Challenge(props, FacebookDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle OAuth callback from external providers
    /// </summary>
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallback(string provider)
    {
        try
        {
            // Authenticate with the external scheme
            var result = await HttpContext.AuthenticateAsync("Identity.External");

            if (!result.Succeeded)
            {
                _logger.LogWarning("External authentication failed for provider {Provider}", provider);
                return Redirect($"{_configuration["FrontendUrl"] ?? "http://localhost:4200"}/login?error=social_auth_failed");
            }

            // Extract user info from claims
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var providerId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profilePicture = result.Principal.FindFirst("picture")?.Value 
                ?? result.Principal.FindFirst("urn:facebook:picture")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("External authentication missing email for provider {Provider}", provider);
                return Redirect($"{_configuration["FrontendUrl"] ?? "http://localhost:4200"}/login?error=email_required");
            }

            // Use the social login service
            var authResponse = await _authenticationService.SocialLogin(
                email, 
                name ?? email.Split('@')[0], 
                provider, 
                providerId ?? string.Empty,
                profilePicture
            );

            // Redirect to frontend with token
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
            return Redirect($"{frontendUrl}/auth/callback?token={authResponse.Token}&refreshToken={authResponse.RefreshToken}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during external authentication callback for provider {Provider}", provider);
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
            return Redirect($"{frontendUrl}/login?error=auth_error");
        }
    }

    #endregion
}
