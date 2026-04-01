using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Features.Social.Requests.Queries;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VoteEngagementController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VoteEngagementController> _logger;

    public VoteEngagementController(IMediator mediator, ILogger<VoteEngagementController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// React to a vote (like, love, insightful)
    /// </summary>
    [HttpPost("votes/{voteId:guid}/react")]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseCommandResponse>> ReactToVote(Guid voteId, [FromBody] ReactionRequest request)
    {
        var response = await _mediator.Send(new ReactToVoteCommand 
        { 
            VoteId = voteId, 
            UserId = GetUserId(), 
            Request = request 
        });

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Add a comment to a vote
    /// </summary>
    [HttpPost("votes/{voteId:guid}/comments")]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<BaseCommandResponse>> AddVoteComment(Guid voteId, [FromBody] AddVoteCommentCommand command)
    {
        command.VoteId = voteId;
        command.UserId = GetUserId();
        
        var response = await _mediator.Send(command);

        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetVoteComments), new { voteId }, response);
    }

    /// <summary>
    /// Like a vote comment
    /// </summary>
    [HttpPost("comments/{commentId:guid}/like")]
    public async Task<ActionResult<BaseCommandResponse>> LikeComment(Guid commentId)
    {
        var response = await _mediator.Send(new LikeVoteCommentCommand 
        { 
            CommentId = commentId, 
            UserId = GetUserId() 
        });

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get comments for a vote
    /// </summary>
    [HttpGet("votes/{voteId:guid}/comments")]
    public async Task<ActionResult<List<VoteCommentDto>>> GetVoteComments(Guid voteId, [FromQuery] int maxDepth = 3)
    {
        var result = await _mediator.Send(new GetVoteCommentsQuery 
        { 
            VoteId = voteId, 
            MaxDepth = maxDepth 
        });
        return Ok(result);
    }
}
