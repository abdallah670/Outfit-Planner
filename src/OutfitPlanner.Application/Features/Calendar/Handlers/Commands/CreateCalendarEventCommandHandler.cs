using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

/// <summary>
/// Handler for creating a new calendar event
/// </summary>
public class CreateCalendarEventCommandHandler 
    : IRequestHandler<CreateCalendarEventCommand, BaseCommandResponse<CalendarEventItemDto>>
{
    private readonly AppDbContext _context;

    public CreateCalendarEventCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BaseCommandResponse<CalendarEventItemDto>> Handle(
        CreateCalendarEventCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse<CalendarEventItemDto>();

        // Validate outfit if provided
        Domain.Entities.Outfit? outfit = null;
        if (request.Request.OutfitId.HasValue)
        {
            outfit = await _context.Outfits
                .FirstOrDefaultAsync(o => o.Id == request.Request.OutfitId.Value && o.UserId == request.UserId, cancellationToken);

            if (outfit == null)
            {
                response.Success = false;
                response.Message = "Outfit not found";
                return response;
            }
        }

        // Create WearEvent if outfit is associated
        Guid? wearEventId = null;
        if (outfit != null)
        {
            var wearEvent = new WearEvent
            {
                UserId = request.UserId,
                OutfitId = outfit.Id,
                WornAt = request.Request.EventDate,
                Notes = request.Request.Notes ?? string.Empty
            };

            _context.WearEvents.Add(wearEvent);
            await _context.SaveChangesAsync(cancellationToken);
            wearEventId = wearEvent.Id;
        }

        // Create Calendar Event
        var calendarEvent = new CalendarEvent
        {
            UserId = request.UserId,
            Title = request.Request.Title,
            Description = request.Request.Description,
            Location = request.Request.Location,
            EventDate = request.Request.EventDate,
            StartTime = request.Request.StartTime,
            EndTime = request.Request.EndTime,
            EventType = (Domain.Entities.CalendarEventType)request.Request.EventType,
            WearEventId = wearEventId,
            Notes = request.Request.Notes,
            IsRecurring = false
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync(cancellationToken);

        // Build response
        response.Success = true;
        response.Message = "Calendar event created successfully";
        response.Id = calendarEvent.Id;
        response.Data = new CalendarEventItemDto
        {
            Id = calendarEvent.Id,
            Title = calendarEvent.Title,
            Description = calendarEvent.Description,
            Location = calendarEvent.Location,
            EventDate = calendarEvent.EventDate,
            StartTime = calendarEvent.StartTime.HasValue 
                ? DateTime.Today.Add(calendarEvent.StartTime.Value).ToString("h:mm tt") 
                : null,
            EndTime = calendarEvent.EndTime.HasValue 
                ? DateTime.Today.Add(calendarEvent.EndTime.Value).ToString("h:mm tt") 
                : null,
            EventType = (CalendarEventType)calendarEvent.EventType,
            WearEventId = calendarEvent.WearEventId,
            OutfitName = outfit?.Name,
            OutfitImageUrl = outfit?.ImageUrl,
            Notes = calendarEvent.Notes,
            IsRecurring = calendarEvent.IsRecurring
        };

        return response;
    }
}

/// <summary>
/// Handler for updating a calendar event
/// </summary>
public class UpdateCalendarEventItemCommandHandler 
    : IRequestHandler<UpdateCalendarEventItemCommand, BaseCommandResponse>
{
    private readonly AppDbContext _context;

    public UpdateCalendarEventItemCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BaseCommandResponse> Handle(
        UpdateCalendarEventItemCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.WearEvent)
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.UserId == request.UserId, cancellationToken);

        if (calendarEvent == null)
        {
            response.Success = false;
            response.Message = "Calendar event not found";
            return response;
        }

        // Update basic properties
        if (request.Request.Title != null)
            calendarEvent.Title = request.Request.Title;
        if (request.Request.Description != null)
            calendarEvent.Description = request.Request.Description;
        if (request.Request.Location != null)
            calendarEvent.Location = request.Request.Location;
        if (request.Request.EventDate.HasValue)
            calendarEvent.EventDate = request.Request.EventDate.Value;
        if (request.Request.StartTime.HasValue)
            calendarEvent.StartTime = request.Request.StartTime.Value;
        if (request.Request.EndTime.HasValue)
            calendarEvent.EndTime = request.Request.EndTime.Value;
        if (request.Request.EventType.HasValue)
            calendarEvent.EventType = (Domain.Entities.CalendarEventType)request.Request.EventType.Value;
        if (request.Request.Notes != null)
            calendarEvent.Notes = request.Request.Notes;

        // Handle outfit association change
        if (request.Request.OutfitId.HasValue)
        {
            var outfit = await _context.Outfits
                .FirstOrDefaultAsync(o => o.Id == request.Request.OutfitId.Value && o.UserId == request.UserId, cancellationToken);

            if (outfit == null)
            {
                response.Success = false;
                response.Message = "Outfit not found";
                return response;
            }

            if (calendarEvent.WearEventId.HasValue)
            {
                // Update existing wear event
                var wearEvent = await _context.WearEvents.FindAsync(calendarEvent.WearEventId.Value, cancellationToken);
                if (wearEvent != null)
                {
                    wearEvent.OutfitId = outfit.Id;
                }
            }
            else
            {
                // Create new wear event
                var wearEvent = new WearEvent
                {
                    UserId = request.UserId,
                    OutfitId = outfit.Id,
                    WornAt = calendarEvent.EventDate,
                    Notes = calendarEvent.Notes ?? string.Empty
                };
                _context.WearEvents.Add(wearEvent);
                await _context.SaveChangesAsync(cancellationToken);
                calendarEvent.WearEventId = wearEvent.Id;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Calendar event updated successfully";
        return response;
    }
}

/// <summary>
/// Handler for deleting a calendar event
/// </summary>
public class DeleteCalendarEventItemCommandHandler 
    : IRequestHandler<DeleteCalendarEventItemCommand, BaseCommandResponse>
{
    private readonly AppDbContext _context;

    public DeleteCalendarEventItemCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BaseCommandResponse> Handle(
        DeleteCalendarEventItemCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var calendarEvent = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.UserId == request.UserId, cancellationToken);

        if (calendarEvent == null)
        {
            response.Success = false;
            response.Message = "Calendar event not found";
            return response;
        }

        // Delete associated wear event if exists
        if (calendarEvent.WearEventId.HasValue)
        {
            var wearEvent = await _context.WearEvents.FindAsync(calendarEvent.WearEventId.Value, cancellationToken);
            if (wearEvent != null)
            {
                _context.WearEvents.Remove(wearEvent);
            }
        }

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Calendar event deleted successfully";
        return response;
    }
}
