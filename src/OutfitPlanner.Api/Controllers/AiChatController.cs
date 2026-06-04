using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;
using System.Security.Claims;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Send a message to the AI fashion assistant and get a response
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<BaseCommandResponse>> Chat([FromBody] ChatCommand command)
    {
        command.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var response = await _mediator.Send(command);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get chat session history
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<List<object>>> GetSessions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        return Ok(new List<object>());
    }
}