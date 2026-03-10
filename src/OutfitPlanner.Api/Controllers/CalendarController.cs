using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;

namespace OutfitPlanner.Api.Controllers;

/// <summary>
/// Controller for calendar operations including outfit scheduling and wear tracking
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(IMediator mediator, ILogger<CalendarController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Get all wear events for a specific month
    /// </summary>
    [HttpGet("events")]
    [ProducesResponseType(typeof(List<CalendarEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetEvents([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        var events = await _mediator.Send(new GetCalendarEventsRequest 
        { 
            UserId = userId, 
            Year = year, 
            Month = month 
        });
        return Ok(events);
    }

    /// <summary>
    /// Get scheduled outfits for a specific month (future events)
    /// </summary>
    [HttpGet("scheduled")]
    [ProducesResponseType(typeof(List<ScheduledOutfitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScheduledOutfitDto>>> GetScheduledOutfits([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        var request = new GetScheduledOutfitsRequest 
        { 
            UserId = userId, 
            Year = year, 
            Month = month 
        };
        var scheduled = await _mediator.Send(request);
        return Ok(scheduled);
    }

    /// <summary>
    /// Get monthly statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(MonthlyStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonthlyStatsDto>> GetMonthlyStats([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        var request = new GetMonthlyStatsRequest 
        { 
            UserId = userId, 
            Year = year, 
            Month = month 
        };
        var stats = await _mediator.Send(request);
        return Ok(stats);
    }

    /// <summary>
    /// Schedule an outfit for a specific date
    /// </summary>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(CalendarEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalendarEventDto>> ScheduleOutfit([FromBody] ScheduleOutfitRequest request)
    {
        var userId = GetUserId();
        
        var command = new ScheduleOutfitCommand
        {
            UserId = userId,
            Request = new ScheduleOutfitRequest
            {
                OutfitId = request.OutfitId,
                ScheduledDate = request.ScheduledDate,
                ClothingItemId = request.ClothingItemId,
                WeatherCondition = request.WeatherCondition,
                Notes = request.Notes
            }
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return BadRequest(new { error = response.Message });

        _logger.LogInformation("User {UserId} scheduled outfit {OutfitId} for {Date}", 
            userId, request.OutfitId, request.ScheduledDate);

        return CreatedAtAction(nameof(GetEvents), new { year = request.ScheduledDate.Year, month = request.ScheduledDate.Month }, 
            new CalendarEventDto { Id = response.Id, OutfitId = request.OutfitId, WornAt = request.ScheduledDate, IsScheduled = true });
    }

    /// <summary>
    /// Update a calendar event
    /// </summary>
    [HttpPut("events/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateEvent(Guid id, [FromBody] UpdateCalendarEventRequest request)
    {
        var userId = GetUserId();
        var command = new UpdateCalendarEventCommand
        {
            Id = id,
            UserId = userId,
            Request = request
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(new { error = response.Message });

        _logger.LogInformation("User {UserId} updated wear event {EventId}", userId, id);
        return Ok(new { message = "Event updated successfully" });
    }

    /// <summary>
    /// Delete a calendar event (unschedule)
    /// </summary>
    [HttpDelete("events/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteEvent(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteCalendarEventCommand
        {
            Id = id,
            UserId = userId
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(new { error = response.Message });

        _logger.LogInformation("User {UserId} deleted wear event {EventId}", userId, id);
        return NoContent();
    }

    /// <summary>
    /// Mark a scheduled outfit as worn
    /// </summary>
    [HttpPost("events/{id:guid}/mark-worn")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> MarkAsWorn(Guid id, [FromBody] MarkAsWornRequest request)
    {
        var userId = GetUserId();
        var command = new MarkAsWornCommand
        {
            Id = id,
            UserId = userId,
            Request = request
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(new { error = response.Message });

        _logger.LogInformation("User {UserId} marked outfit as worn for event {EventId}", userId, id);
        return Ok(new { message = "Marked as worn successfully" });
    }

    #region Calendar Events (Time-based events like "Team Meeting at 2:00 PM")

    /// <summary>
    /// Get calendar events with time slots for a specific date (for sidebar display)
    /// </summary>
    [HttpGet("calendar-events/by-date")]
    [ProducesResponseType(typeof(List<CalendarEventItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CalendarEventItemDto>>> GetCalendarEventsByDate([FromQuery] DateTimeOffset date)
    {
        var userId = GetUserId();
        var events = await _mediator.Send(new GetCalendarEventsByDateRequest 
        { 
            UserId = userId, 
            Date = date 
        });
        return Ok(events);
    }

    /// <summary>
    /// Get calendar events for a specific month
    /// </summary>
    [HttpGet("calendar-events")]
    [ProducesResponseType(typeof(List<CalendarEventItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CalendarEventItemDto>>> GetCalendarEventsForMonth(
        [FromQuery] int year, 
        [FromQuery] int month)
    {
        var userId = GetUserId();
        var events = await _mediator.Send(new GetCalendarEventsForMonthRequest 
        { 
            UserId = userId, 
            Year = year, 
            Month = month 
        });
        return Ok(events);
    }

    /// <summary>
    /// Create a new calendar event (with optional outfit association)
    /// </summary>
    [HttpPost("calendar-events")]
    [ProducesResponseType(typeof(CalendarEventItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalendarEventItemDto>> CreateCalendarEvent(
        [FromBody] CreateCalendarEventRequest request)
    {
        var userId = GetUserId();
        
        var command = new CreateCalendarEventCommand
        {
            UserId = userId,
            Request = request
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return BadRequest(new { error = response.Message });

        _logger.LogInformation("User {UserId} created calendar event '{Title}' for {Date}", 
            userId, request.Title, request.EventDate);

        return CreatedAtAction(nameof(GetCalendarEventsByDate), 
            new { date = request.EventDate }, 
            response.Data);
    }

    /// <summary>
    /// Update a calendar event
    /// </summary>
    [HttpPut("calendar-events/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateCalendarEvent(
        Guid id, 
        [FromBody] UpdateCalendarEventItemRequest request)
    {
        var userId = GetUserId();
        var command = new UpdateCalendarEventItemCommand
        {
            Id = id,
            UserId = userId,
            Request = request
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(new { error = response.Message });

        _logger.LogInformation("User {UserId} updated calendar event {EventId}", userId, id);
        return Ok(new { message = "Calendar event updated successfully" });
    }

    /// <summary>
    /// Delete a calendar event
    /// </summary>
    [HttpDelete("calendar-events/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteCalendarEvent(Guid id)
    {
        var userId = GetUserId();
        var command = new DeleteCalendarEventItemCommand
        {
            Id = id,
            UserId = userId
        };

        var response = await _mediator.Send(command);

        if (!response.Success)
            return NotFound(new { error = response.Message });

        _logger.LogInformation("User {UserId} deleted calendar event {EventId}", userId, id);
        return NoContent();
    }

    #endregion
}
