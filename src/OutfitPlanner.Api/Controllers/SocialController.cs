using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Features.Social.Requests.Queries;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Api.Controllers;

/// <summary>
/// Controller for social features including polls, voting, and trends
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SocialController> _logger;

    public SocialController(IMediator mediator, ILogger<SocialController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    
    /// <summary>
    /// Gets all polls for the authenticated user
    /// </summary>
    [HttpGet("polls")]
    [ProducesResponseType(typeof(List<ValidationPollDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ValidationPollDto>>> GetPolls()
    {
        var userId = GetUserId();
        var polls = await _mediator.Send(new GetPollsRequest { UserId = userId });
        return Ok(polls);
    }

    /// <summary>
    /// Gets a specific poll by ID
    /// </summary>
    [HttpGet("polls/{id:guid}")]
    [ProducesResponseType(typeof(ValidationPollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ValidationPollDto>> GetPollById(Guid id)
    {
        var userId = GetUserId();
        var poll = await _mediator.Send(new GetPollByIdRequest { Id = id, UserId = userId });

        if (poll == null)
            return NotFound();

        return Ok(poll);
    }

    /// <summary>
    /// Creates a new validation poll
    /// </summary>
    [HttpPost("polls")]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseCommandResponse>> CreatePoll([FromBody] CreatePollDto request)
    {
        var userId = GetUserId();
        var command = new CreatePollCommand { UserId = userId, Request = request };
        var response = await _mediator.Send(command);

        if (!response.Success)
            return BadRequest(response);

        _logger.LogInformation("User {UserId} created poll {PollId}", userId, response.Id);
        return CreatedAtAction(nameof(GetPollById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Casts a vote on a poll
    /// </summary>
    [HttpPost("polls/{id:guid}/vote")]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseCommandResponse>> Vote(Guid id, [FromBody] CastVoteDto request)
    {
        var userId = GetUserId();
        var command = new VoteOnPollCommand { UserId = userId, PollId = id, Request = request };
        var response = await _mediator.Send(command);

        if (!response.Success)
            return BadRequest(response);

        _logger.LogInformation("User {UserId} voted on poll {PollId}", userId, id);
        return Ok(response);
    }

    /// <summary>
    /// Updates an existing poll (only the owner can edit)
    /// </summary>
    [HttpPut("polls/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdatePoll(Guid id, [FromBody] UpdatePollRequest request)
    {
        var userId = GetUserId();
        var command = new UpdatePollCommand
        {
            Id = id,
            UserId = userId,
            Request = request
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
                return NotFound(new { error = response.Message });
            return Forbid();
        }

        _logger.LogInformation("User {UserId} updated poll {PollId}", userId, id);
        return Ok(new { message = "Poll updated successfully" });
    }

    /// <summary>
    /// Deletes a poll (only the owner can delete)
    /// </summary>
    [HttpDelete("polls/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeletePoll(Guid id)
    {
        var userId = GetUserId();
        var command = new DeletePollCommand
        {
            Id = id,
            UserId = userId
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
                return NotFound(new { error = response.Message });
            return Forbid();
        }

        _logger.LogInformation("User {UserId} deleted poll {PollId}", userId, id);
        return NoContent();
    }

    /// <summary>
    /// Closes a poll (only the owner can close)
    /// </summary>
    [HttpPost("polls/{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ClosePoll(Guid id)
    {
        var userId = GetUserId();
        var command = new ClosePollCommand
        {
            Id = id,
            UserId = userId
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
                return NotFound(new { error = response.Message });
            return Forbid();
        }

        _logger.LogInformation("User {UserId} closed poll {PollId}", userId, id);
        return Ok(new { message = "Poll closed successfully" });
    }

    /// <summary>
    /// Gets local trending polls and topics
    /// </summary>
    [HttpGet("trends/local")]
    [ProducesResponseType(typeof(TrendingDataDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrendingDataDto>> GetLocalTrends()
    {
        var result = await _mediator.Send(new GetLocalTrendsRequest());
        return Ok(result);
    }

    /// <summary>
    /// Gets trending outfits from the community
    /// </summary>
    [HttpGet("trending-outfits")]
    [ProducesResponseType(typeof(List<TrendingOutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TrendingOutfitDto>>> GetTrendingOutfits()
    {
        var result = await _mediator.Send(new GetTrendingOutfitsRequest());
        return Ok(result);
    }
}
