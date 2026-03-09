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
    /// Gets local trending polls and topics (placeholder implementation)
    /// </summary>
    [HttpGet("trends/local")]
    [ProducesResponseType(typeof(TrendingDataDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrendingDataDto>> GetLocalTrends()
    {
        // Placeholder implementation - returns hardcoded trending data
        var trends = new TrendingDataDto
        {
            Trends = new List<TrendItemDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Summer Fashion Trends 2026",
                    Description = "Discover the hottest summer styles",
                    Category = "Seasonal",
                    PopularityScore = 95,
                    TrendingSince = DateTimeOffset.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Sustainable Fashion",
                    Description = "Eco-friendly outfit ideas",
                    Category = "Lifestyle",
                    PopularityScore = 87,
                    TrendingSince = DateTimeOffset.UtcNow.AddDays(-5)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Office Casual",
                    Description = "Work-appropriate casual wear",
                    Category = "Occasion",
                    PopularityScore = 82,
                    TrendingSince = DateTimeOffset.UtcNow.AddDays(-2)
                }
            },
            TopPolls = new List<TopPollDto>
            {
                new()
                {
                    PollId = Guid.NewGuid(),
                    Question = "Which color palette for spring?",
                    TotalVotes = 156,
                    EngagementRate = 0.78
                },
                new()
                {
                    PollId = Guid.NewGuid(),
                    Question = "Best outfit for a first date?",
                    TotalVotes = 243,
                    EngagementRate = 0.85
                }
            },
            GeneratedAt = DateTimeOffset.UtcNow
        };

        return Ok(trends);
    }
}

/// <summary>
/// DTO for trending data response
/// </summary>
public class TrendingDataDto
{
    public List<TrendItemDto> Trends { get; set; } = new();
    public List<TopPollDto> TopPolls { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
}

/// <summary>
/// DTO for a single trend item
/// </summary>
public class TrendItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int PopularityScore { get; set; }
    public DateTimeOffset TrendingSince { get; set; }
}

/// <summary>
/// DTO for top poll information in trends
/// </summary>
public class TopPollDto
{
    public Guid PollId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public double EngagementRate { get; set; }
}
