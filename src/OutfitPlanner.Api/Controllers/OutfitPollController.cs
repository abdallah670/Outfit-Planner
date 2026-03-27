using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Social.Requests.Commands;
using OutfitPlanner.Application.Features.Social.Requests.Queries;

namespace OutfitPlanner.Api.Controllers;

/// <summary>
/// Controller for outfit engagement (like, comment)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutfitPollController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutfitPollController> _logger;

    public OutfitPollController(IMediator mediator, ILogger<OutfitPollController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Like an outfit
    /// </summary>
    [HttpPost("outfits/{outfitId:guid}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutfitVoteResultDto>> LikeOutfit(Guid outfitId)
    {
        var command = new LikeOutfitCommand
        {
            OutfitId = outfitId,
            UserId = GetUserId()
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(response.Message);

        // Fetch updated counts
        var engagement = await _mediator.Send(new GetOutfitEngagementQuery 
        { 
            OutfitId = outfitId, 
            UserId = command.UserId 
        });

        return Ok(new OutfitVoteResultDto
        {
            OutfitId = outfitId,
            VoteCount = engagement.LikeCount,
            UserHasVoted = engagement.UserHasLiked
        });
    }

    /// <summary>
    /// Unlike an outfit
    /// </summary>
    [HttpDelete("outfits/{outfitId:guid}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutfitVoteResultDto>> UnlikeOutfit(Guid outfitId)
    {
        var command = new UnlikeOutfitCommand
        {
            OutfitId = outfitId,
            UserId = GetUserId()
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(response.Message);

        // Fetch updated counts
        var engagement = await _mediator.Send(new GetOutfitEngagementQuery 
        { 
            OutfitId = outfitId, 
            UserId = command.UserId 
        });

        return Ok(new OutfitVoteResultDto
        {
            OutfitId = outfitId,
            VoteCount = engagement.LikeCount,
            UserHasVoted = engagement.UserHasLiked
        });
    }

    /// <summary>
    /// Comment on an outfit
    /// </summary>
    [HttpPost("outfits/{outfitId:guid}/comment")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitCommentDto>> CommentOnOutfit(Guid outfitId, [FromBody] CreateCommentRequest request)
    {
        var command = new AddOutfitCommentCommand
        {
            OutfitId = outfitId,
            UserId = GetUserId(),
            Content = request.Content
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return BadRequest(response.Message);

        // We could fetch the specific comment, but returning a success response with the ID is often enough.
        // For simplicity returning a placeholder DTO. The frontend usually re-fetches or adds it optimistically.
        return CreatedAtAction(nameof(GetOutfitVotes), new { outfitId }, new OutfitCommentDto
        {
            Id = response.Id,
            OutfitId = outfitId,
            UserId = command.UserId,
            Content = command.Content,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Get all comments for an outfit
    /// </summary>
    [HttpGet("outfits/{outfitId:guid}/votes")] // Kept original path to not break frontend
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OutfitCommentsResponse>> GetOutfitVotes(Guid outfitId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetOutfitCommentsQuery
        {
            OutfitId = outfitId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        
        // Return wrapped in the structure the frontend expects (Items instead of PagedResult direct)
        return Ok(new OutfitCommentsResponse 
        { 
            Items = result.Items.ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    /// <summary>
    /// Get engagement stats for an outfit
    /// </summary>
    [HttpGet("outfits/{outfitId:guid}/engagement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OutfitEngagementDto>> GetEngagement(Guid outfitId)
    {
        var query = new GetOutfitEngagementQuery
        {
            OutfitId = outfitId,
            UserId = GetUserId()
        };

        var result = await _mediator.Send(query);

        return Ok(new OutfitEngagementDto
        {
            VoteCount = result.LikeCount,
            CommentCount = result.CommentCount,
            ReactionCount = 0, // Reactions deprecated
            UserHasVoted = result.UserHasLiked,
            UserReaction = null
        });
    }
}

// Wrapper to preserve exact JSON structure for frontend backwards compatibility
public class OutfitCommentsResponse
{
    public List<OutfitCommentDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
