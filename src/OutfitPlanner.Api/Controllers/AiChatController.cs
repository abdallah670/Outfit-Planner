using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
[EnableRateLimiting("Api")]
public class AiChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IChatSessionRepository _sessionRepository;

    public AiChatController(IMediator mediator, IChatSessionRepository sessionRepository)
    {
        _mediator = mediator;
        _sessionRepository = sessionRepository;
    }
    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Send a message to the AI fashion assistant and get a response
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<BaseCommandResponse>> Chat([FromForm] ChatCommand command)
    {
        command.UserId = GetUserId();

        var response = await _mediator.Send(command);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get chat session history
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<List<OutfitPlanner.Application.DTOs.AI.ChatSessionDto>>> GetSessions()
    {
        var userId = GetUserId();
        var query = new OutfitPlanner.Application.Features.AI.Requests.Queries.GetSessionsQuery { UserId = userId };
        var sessions = await _mediator.Send(query);
        return Ok(sessions);
    }

    /// <summary>
    /// Get messages for a specific session
    /// </summary>
    [HttpGet("sessions/{id}/messages")]
    public async Task<ActionResult<List<ChatMessage>>> GetSessionMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var session = await _sessionRepository.GetByIdAsync(id); // Use GetByIdAsync instead of GetByIdWithMessagesAsync
        if (session == null)
            return NotFound();

        if (session.UserId != userId)
            return Forbid();

        var skip = (page - 1) * pageSize;
        var messages = await _sessionRepository.GetMessagesBySessionIdAsync(id, skip, pageSize);
        
        return Ok(messages);
    }

    /// <summary>
    /// Delete a chat session and all its messages
    /// </summary>
    [HttpDelete("sessions/{id}")]
    public async Task<ActionResult<BaseCommandResponse>> DeleteSession(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new DeleteSessionCommand { UserId = userId, SessionId = id };
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
