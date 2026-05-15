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

    /// <summary>
    /// Get Posts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CursorPagination.CursorPagedResult<FeedPostDto>>> GetFeedPosts(
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] string visibility = "Public",
        [FromQuery] string? sortBy = "recent",
        [FromQuery] string? postType = null)
    {
        var query = new GetFeedQuery 
        { 
            UserId = null, 
            Cursor = cursor, 
            PageSize = pageSize,
            SortBy = sortBy ?? "recent",
            Visibility = visibility,
            PostType = postType 
        };
        
        var posts = await _mediator.Send(query);
        return Ok(posts);
    }

    /// <summary>
    /// Get posts created by a specific user (for public profile activity)
    /// </summary>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<CursorPagination.CursorPagedResult<FeedPostDto>>> GetUserFeed(
        string userId,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? postType = null)
    {
        try
        {
            var viewerId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
            var query = new GetUserFeedQuery
            {
                UserId = userId,
                ViewerUserId = viewerId,
                Cursor = cursor,
                PageSize = pageSize,
                PostType = postType
            };
            
            var posts = await _mediator.Send(query);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feed for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to retrieve user feed" });
        }
    }

    /// <summary>
    /// Get my own posts
    /// </summary>
    [HttpGet("my-posts")]
    public async Task<ActionResult<CursorPagination.CursorPagedResult<FeedPostDto>>> GetMyPosts(
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "recent",
        [FromQuery] string? postType = null)
    {
        var userId = GetUserId();
        var query = new GetUserFeedQuery
        {
            UserId = userId,
            Cursor = cursor,
            PageSize = pageSize,
            PostType = postType
        };
        
        var posts = await _mediator.Send(query);
        return Ok(posts);
    }

    /// <summary>
    /// Get a specific post by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetFeedPostByIdDto?>> GetPostById(Guid id)
    {
        var userId = GetUserId();
        var query = new GetFeedPostByIdQuery { PostId = id, RequesterId = userId };
        var post = await _mediator.Send(query);
        
        if (post == null)
            return NotFound();
            
        return Ok(post);
    }

    /// <summary>
    /// Delete a post (works for both outfit posts and poll posts)
    /// </summary>
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

    /// <summary>
    /// Add heart reaction to a post
    /// </summary>
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

    /// <summary>
    /// Remove heart reaction from a post
    /// </summary>
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

    /// <summary>
    /// Get comments on a post with cursor-based pagination
    /// </summary>
    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<CursorPagination.CursorPagedResult<PostCommentDto>>> GetComments(
        Guid id, 
        [FromQuery] string? cursor = null, 
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPostCommentsQuery
        {
            PostId = id,
            Cursor = cursor,
            PageSize = pageSize
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Add a comment to a post
    /// </summary>
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

    /// <summary>
    /// Delete a comment from a post
    /// </summary>
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

    /// <summary>
    /// Update a comment on a post
    /// </summary>
    [HttpPut("comments/{commentId:guid}")]
    public async Task<ActionResult<BaseCommandResponse>> UpdateComment(Guid commentId, [FromBody] CreateCommentDto request)
    {
        var userId = GetUserId();
        var command = new UpdatePostCommentCommand
        {
            CommentId = commentId,
            UserId = userId,
            Content = request.Content
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }
}