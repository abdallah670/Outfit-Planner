using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using DomainCalendarEventType = OutfitPlanner.Domain.Entities.CalendarEventType;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

/// <summary>
/// Handler for creating a new calendar event
/// </summary>
public class CreateCalendarEventCommandHandler 
    : IRequestHandler<CreateCalendarEventCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCalendarEventCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(
        CreateCalendarEventCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        // Validate outfit if provided
        Outfit? outfit = null;
        if (request.Request.OutfitId.HasValue)
        {
            outfit = await _unitOfWork.Outfits.GetByIdAsync(request.Request.OutfitId.Value);

            if (outfit == null || outfit.UserId != request.UserId)
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

            await _unitOfWork.WearEvents.AddAsync(wearEvent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
            EventType = (DomainCalendarEventType)request.Request.EventType,
            WearEventId = wearEventId,
            Notes = request.Request.Notes,
            IsRecurring = false
        };

        await _unitOfWork.CalendarEvents.AddAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Calendar event created successfully";
        response.Id = calendarEvent.Id;

        return response;
    }
}

/// <summary>
/// Handler for updating a calendar event
/// </summary>
public class UpdateCalendarEventItemCommandHandler 
    : IRequestHandler<UpdateCalendarEventItemCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCalendarEventItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(
        UpdateCalendarEventItemCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(request.Id);

        if (calendarEvent == null || calendarEvent.UserId != request.UserId)
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
            calendarEvent.EventType = (DomainCalendarEventType)request.Request.EventType.Value;
        if (request.Request.Notes != null)
            calendarEvent.Notes = request.Request.Notes;

        // Handle outfit association change
        if (request.Request.OutfitId.HasValue)
        {
            var outfit = await _unitOfWork.Outfits.GetByIdAsync(request.Request.OutfitId.Value);

            if (outfit == null || outfit.UserId != request.UserId)
            {
                response.Success = false;
                response.Message = "Outfit not found";
                return response;
            }

            if (calendarEvent.WearEventId.HasValue)
            {
                // Update existing wear event
                var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(calendarEvent.WearEventId.Value);
                if (wearEvent != null)
                {
                    wearEvent.OutfitId = outfit.Id;
                    await _unitOfWork.WearEvents.UpdateAsync(wearEvent);
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
                await _unitOfWork.WearEvents.AddAsync(wearEvent);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                calendarEvent.WearEventId = wearEvent.Id;
            }
        }

        await _unitOfWork.CalendarEvents.UpdateAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCalendarEventItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(
        DeleteCalendarEventItemCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(request.Id);

        if (calendarEvent == null || calendarEvent.UserId != request.UserId)
        {
            response.Success = false;
            response.Message = "Calendar event not found";
            return response;
        }

        // Delete associated wear event if exists
        if (calendarEvent.WearEventId.HasValue)
        {
            var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(calendarEvent.WearEventId.Value);
            if (wearEvent != null)
            {
                await _unitOfWork.WearEvents.RemoveAsync(wearEvent);
            }
        }

        await _unitOfWork.CalendarEvents.RemoveAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Calendar event deleted successfully";
        return response;
    }
}
