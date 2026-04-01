using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeedController> _logger;

    public FeedController(IMediator mediator, ILogger<FeedController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<FeedPostResponse>> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "popular")
    {
        var userId = GetUserId();
        var query = new GetFeedQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            Visibility = Visibility.Public
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("outfit-post")]
    public async Task<ActionResult<BaseCommandResponse>> CreateOutfitPost([FromBody] CreateOutfitPostDto request)
    {
        var userId = GetUserId();
        var command = new CreateOutfitPostCommand
        {
            UserId = userId,
            OutfitId = request.OutfitId,
            Caption = request.Caption,
            Visibility = request.Visibility
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        _logger.LogInformation("User {UserId} created outfit post {PostId}", userId, response.Id);
        return CreatedAtAction(nameof(GetPostById), new { id = response.Id }, response);
    }

    [HttpPost("poll-post")]
    public async Task<ActionResult<BaseCommandResponse>> CreatePollPost([FromBody] CreatePollPostDto request)
    {
        var userId = GetUserId();
        var command = new CreatePollPostCommand
        {
            UserId = userId,
            Question = request.Question,
            OutfitIds = request.OutfitIds,
            ExpiresAt = request.ExpiresAt,
            Visibility = request.Visibility
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        _logger.LogInformation("User {UserId} created poll post {PostId}", userId, response.Id);
        return CreatedAtAction(nameof(GetPostById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeedPostDto>> GetPostById(Guid id)
    {
        var userId = GetUserId();
        var query = new GetFeedPostByIdQuery { PostId = id, UserId = userId };
        var post = await _mediator.Send(query);
        
        if (post == null)
            return NotFound();
            
        return Ok(post);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteFeedPostCommand { PostId = id, UserId = userId };
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return NotFound(response.Message);
            
        return NoContent();
    }

    [HttpPost("{id:guid}/heart")]
    public async Task<ActionResult<BaseCommandResponse>> AddReaction(Guid id)
    {
        var userId = GetUserId();
        var command = new AddPostReactionCommand
        {
            PostId = id,
            UserId = userId,
            ReactionType = "Heart"
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [HttpDelete("{id:guid}/heart")]
    public async Task<ActionResult<BaseCommandResponse>> RemoveReaction(Guid id)
    {
        var userId = GetUserId();
        var command = new RemovePostReactionCommand
        {
            PostId = id,
            UserId = userId
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<PostCommentsResponse>> GetComments(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetPostCommentsQuery
        {
            PostId = id,
            Page = page,
            PageSize = pageSize
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<BaseCommandResponse>> AddComment(Guid id, [FromBody] CreateCommentDto request)
    {
        var userId = GetUserId();
        var command = new AddPostCommentCommand
        {
            PostId = id,
            UserId = userId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return CreatedAtAction(nameof(GetComments), new { id }, response);
    }

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetUserId();
        var command = new DeletePostCommentCommand
        {
            CommentId = commentId,
            UserId = userId
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return NotFound(response.Message);
            
        return NoContent();
    }
}