using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using System.Linq;
using System.Collections.Generic;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PollsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PollsController> _logger;
    private readonly IVoteRepository _voteRepository;

    public PollsController(IMediator mediator, ILogger<PollsController> logger, IVoteRepository voteRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _voteRepository = voteRepository;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Get all polls created by the current user
    /// </summary>
    [HttpGet("my-polls")]
    public async Task<ActionResult<List<ValidationPollDto>>> GetMyPolls([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var query = new GetPollsRequest { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific poll by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ValidationPollDto>> GetPoll(Guid id)
    {
        var query = new GetPollByIdRequest { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new poll
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseCommandResponse>> CreatePoll([FromBody] CreatePollPostDto request)
    {
        var userId = GetUserId();
        var command = new CreatePollPostCommand
        {
            UserId = userId,
            Question = request.Question,
            Options = request.Options,
            Context = request.Context,
            ExpiresAt = request.ExpiresAt,
            Visibility = request.Visibility,
            Tags = request.Tags
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        _logger.LogInformation("User {UserId} created poll {PollId}", userId, response.Id);
        return CreatedAtAction(nameof(GetPoll), new { id = response.Id }, response);
    }

    /// <summary>
    /// Update an existing poll
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BaseCommandResponse>> UpdatePoll(Guid id, [FromBody] UpdatePollPostDto request)
    {
        var userId = GetUserId();
        var command = new UpdatePollPostCommand
        {
            PostId = id,
            UserId = userId,
            Question = request.Question,
            ExpiresAt = request.ExpiresAt,
            Visibility = request.Visibility,
            Tags = request.Tags,
            Options = request.Options,
            Context = request.Context
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    /// <summary>
    /// Delete a poll
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePoll(Guid id)
    {
        var userId = GetUserId();
        var command = new DeletePollPostCommand { PostId = id, UserId = userId };
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return NotFound(response.Message);
            
        return NoContent();
    }

    /// <summary>
    /// Vote on a poll option
    /// </summary>
    [HttpPost("{id:guid}/vote")]
    public async Task<ActionResult<BaseCommandResponse>> VoteOnPoll(Guid id, [FromBody] CastVoteDto request)
    {
        var userId = GetUserId();
        var command = new VoteOnPollCommand
        {
            PollId = id,
            UserId = userId,
            Request = request
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [HttpDelete("vote")]
    public async Task<ActionResult<BaseCommandResponse>> UnVoteOnPoll([FromBody]Guid optionId)
    {
        var userId = GetUserId();
        var uncastVoteRequest = new unCastVoteDto { OptionId = optionId };
        var command = new UnVoteOnPollCommand {   UserId = userId, Request = uncastVoteRequest };
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return NotFound(response.Message);
            
        return NoContent();
    }

    /// <summary>
    /// Close a poll (stop accepting votes)
    /// </summary>
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<BaseCommandResponse>> ClosePoll(Guid id)
    {
        var userId = GetUserId();
        var command = new ClosePollCommand { Id = id, UserId = userId };
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }
    /// <summary>
    /// Get the most voted active poll with cursor-paginated comments
    /// </summary>
    [HttpGet("recent-poll")]
    [AllowAnonymous]
    public async Task<ActionResult<RecentPollWithCommentsDto>> GetRecentPollWithComments(
        [FromQuery] string? commentsCursor = null,
        [FromQuery] int commentsPageSize = 20)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
        
        var query = new GetRecentPollWithCommentsQuery
        {
            UserId = userId,
            CommentsCursor = commentsCursor,
            CommentsPageSize = commentsPageSize
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get voters for a poll, optionally filtered by option
    /// </summary>
    [HttpGet("{pollId:guid}/voters")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> GetPollVoters(Guid pollId, [FromQuery] Guid? optionId = null)
    {
        var query = new GetPollVotersQuery
        {
            PollId = pollId,
            OptionId = optionId
        };

        var voters = await _mediator.Send(query);

        var result = voters.Select(v => new
        {
            voterId = v.Vote.VoterId,
            voterName = v.VoterName,
            voterAvatarUrl = v.VoterAvatarUrl,
            votedAt = v.Vote.CreatedAt,
            optionId = v.Vote.OptionId,
            optionDescription = !string.IsNullOrEmpty(v.Vote.Option.Description)
                ? v.Vote.Option.Description
                : (v.Vote.Option.Outfit?.Name ?? string.Empty),
            optionDisplayOrder = v.Vote.Option.DisplayOrder
        });

        return Ok(result);
    }
    
}