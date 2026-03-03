using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutfitsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutfitsController> _logger;

    public OutfitsController(IMediator mediator, ILogger<OutfitsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Gets all outfits for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OutfitDto>>> GetAll()
    {
        var userId = GetUserId();
        var outfits = await _mediator.Send(new GetOutfitsRequest { UserId = userId });
        return Ok(outfits);
    }

    /// <summary>
    /// Gets a specific outfit by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutfitDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var outfit = await _mediator.Send(new GetOutfitByIdRequest { Id = id, UserId = userId });
        
        if (outfit == null)
            return NotFound();

        return Ok(outfit);
    }

    /// <summary>
    /// Creates a new outfit
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitDto>> Create([FromBody] CreateOutfitDto request)
    {
        var userId = GetUserId();
        var command = new CreateOutfitCommand { UserId = userId, Request = request };
        var response = await _mediator.Send(command);

        if (response == null)
            return BadRequest("Failed to create outfit");

        _logger.LogInformation("User {UserId} created outfit {OutfitId}", userId, response.Id);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing outfit
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseCommandResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitDto>> Update(Guid id, [FromBody] UpdateOutfitDto request)
    {
        var userId = GetUserId();
        var command = new UpdateOutfitCommand { Id = id, UserId = userId, Request = request };
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("User {UserId} updated outfit {OutfitId}", userId, id);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an outfit
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteOutfitCommand { Id = id, UserId = userId };
        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(response);

        _logger.LogInformation("User {UserId} deleted outfit {OutfitId}", userId, id);
        return NoContent();
    }

    /// <summary>
    /// Records a wear event for an outfit
    /// </summary>
    [HttpPost("{id:guid}/wear")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitDto>> RecordWear(Guid id, [FromBody] RecordOutfitWearDto dto)
    {
        var userId = GetUserId();
        var command = new RecordOutfitWearCommand 
        { 
            UserId = userId, 
            OutfitId = id,
            WornAt = dto.WornAt,
            WeatherCondition = dto.WeatherCondition,
            EventId = dto.EventId
        };
        var response = await _mediator.Send(command);

        if (response == null)
            return BadRequest("Failed to record wear");

        return Ok(response);
    }

    /// <summary>
    /// Gets a single outfit suggestion for today
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(OutfitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutfitDto>> GetToday()
    {
        var userId = GetUserId();
        // Today suggestion - simplified for now, usually would fetch weather first
        var query = new GenerateOutfitSuggestionsQuery 
        { 
            UserId = userId, 
            OutfitSuggestionsDto = new OutfitSuggestionsDto { MaxSuggestions = 1 } 
        };
        var suggestions = await _mediator.Send(query);
        
        if (suggestions == null || !suggestions.Any())
            return NotFound("No suitable outfit found for today.");

        return Ok(suggestions.First());
    }

    /// <summary>
    /// Generates outfit suggestions based on criteria
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(List<OutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OutfitDto>>> GenerateSuggestions([FromBody] OutfitSuggestionsDto dto)
    {
        var userId = GetUserId();
        var query = new GenerateOutfitSuggestionsQuery 
        { 
            UserId = userId, 
            OutfitSuggestionsDto = dto 
        };
        var suggestions = await _mediator.Send(query);
        return Ok(suggestions);
    }
}
