using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutfitPostsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutfitPostsController> _logger;

    public OutfitPostsController(IMediator mediator, ILogger<OutfitPostsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Create a new outfit post
    /// </summary>
    [HttpPost]
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
        return CreatedAtAction(nameof(GetOutfitPost), new { id = response.Id }, response);
    }

    /// <summary>
    /// Get a specific outfit post by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeedPostDto>> GetOutfitPost(Guid id)
    {
        var userId = GetUserId();
        var query = new GetFeedPostByIdQuery { PostId = id, RequesterId = userId };
        var post = await _mediator.Send(query);
        
        if (post == null)
            return NotFound();
            
        return Ok(post);
    }

    /// <summary>
    /// Update an outfit post
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BaseCommandResponse>> UpdateOutfitPost(Guid id, [FromBody] UpdateOutfitPostDto request)
    {
        var userId = GetUserId();
        var command = new UpdateOutfitPostCommand
        {
            PostId = id,
            UserId = userId,
            Caption = request.Caption,
            Visibility = request.Visibility
        };
        
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    /// <summary>
    /// Delete an outfit post
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteOutfitPost(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteFeedPostCommand { PostId = id, UserId = userId };
        var response = await _mediator.Send(command);
        
        if (!response.Success)
            return NotFound(response.Message);
            
        return NoContent();
    }

   
}