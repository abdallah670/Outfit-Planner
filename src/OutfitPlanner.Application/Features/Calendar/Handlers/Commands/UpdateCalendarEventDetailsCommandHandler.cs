using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using DomainCalendarEventType = OutfitPlanner.Domain.Enums.CalendarEventType;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

/// <summary>
/// Handler for updating CalendarEvent properties (Title, Description, Location, etc.)
/// </summary>
public class UpdateCalendarEventDetailsCommandHandler 
    : IRequestHandler<UpdateCalendarEventDetailsCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCalendarEventDetailsCommandHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateCalendarEventDetailsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateCalendarEventDetailsCommandHandler> logger,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(
        UpdateCalendarEventDetailsCommand request, 
        CancellationToken cancellationToken)
    {
        // Validate request
        var validator = new UpdateCalendarEventDetailsCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for update calendar event details request for user with ID {UserId}. Errors: {Errors}", request.UserId, errors);
            throw new ValidationException(validationResult);
        }

        var response = new BaseCommandResponse();

        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(request.Id);

        if (calendarEvent == null)
        {
            _logger.LogWarning("Calendar event with ID {CalendarEventId} not found", request.Id);
            throw new NotFoundException("Calendar event", request.Id);
        }

        if (calendarEvent.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to update calendar event {CalendarEventId} belonging to another user", request.UserId, request.Id);
            throw new Exceptions.UnauthorizedAccessException("You are not authorized to update this event");
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
