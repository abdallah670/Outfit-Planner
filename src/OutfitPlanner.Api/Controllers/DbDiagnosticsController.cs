using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Models.Identity;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DbDiagnosticsController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DbDiagnosticsController> _logger;

    public DbDiagnosticsController(UserManager<User> userManager, ILogger<DbDiagnosticsController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] AuthRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return NotFound("User not found");

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
            _logger.LogError("RemovePassword failed for {Email}: {Errors}", request.Email, errors);
            return BadRequest(removeResult.Errors);
        }

        var addResult = await _userManager.AddPasswordAsync(user, request.Password);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            _logger.LogError("AddPassword failed for {Email}: {Errors}", request.Email, errors);
            return BadRequest(addResult.Errors);
        }

        return Ok($"Password for {request.Email} has been reset to {request.Password}");
    }
}
