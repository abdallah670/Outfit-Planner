using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using DomainCalendarEventType = OutfitPlanner.Domain.Enums.CalendarEventType;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

/// <summary>
/// Handler for creating a new calendar event
/// </summary>
public class CreateCalendarEventCommandHandler 
    : IRequestHandler<CreateCalendarEventCommand, BaseCommandResponse>
{
    private readonly ILogger<CreateCalendarEventCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCalendarEventCommandHandler(
        ILogger<CreateCalendarEventCommandHandler> logger,
        IUnitOfWork unitOfWork, 
        IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(
        CreateCalendarEventCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        // Validate request
        var validationResult = await new CreateCalendarEventCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for calendar event creation request for user with ID {UserId}. Errors: {Errors}", request.UserId, errors);
            throw new ValidationException(validationResult);
        }

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
