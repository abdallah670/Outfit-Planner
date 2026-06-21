using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.Features.AI.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using System.Security.Claims;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IChatSessionRepository _sessionRepository;

    public AiChatController(IMediator mediator, IChatSessionRepository sessionRepository)
    {
        _mediator = mediator;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Send a message to the AI fashion assistant and get a response
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<BaseCommandResponse>> Chat([FromForm] ChatCommand command)
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
    public async Task<ActionResult<List<OutfitPlanner.Application.DTOs.AI.ChatSessionDto>>> GetSessions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var query = new OutfitPlanner.Application.Features.AI.Requests.Queries.GetSessionsQuery { UserId = userId };
        var sessions = await _mediator.Send(query);
        return Ok(sessions);
    }

    /// <summary>
    /// Get messages for a specific session
    /// </summary>
    [HttpGet("sessions/{id}/messages")]
    public async Task<ActionResult<List<ChatMessage>>> GetSessionMessages(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var session = await _sessionRepository.GetByIdWithMessagesAsync(id);
        if (session == null)
            return NotFound();

        if (session.UserId != userId)
            return Forbid();

        return Ok(session.Messages?.OrderBy(m => m.CreatedAt).ToList() ?? new List<ChatMessage>());
    }
}