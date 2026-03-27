using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Queries;

/// <summary>
/// Handler for getting a single calendar event by ID with full details including outfit info
/// </summary>
public class GetCalendarEventByIdRequestHandler 
    : IRequestHandler<GetCalendarEventByIdRequest, CalendarEventItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCalendarEventByIdRequestHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CalendarEventItemDto> Handle(
        GetCalendarEventByIdRequest request, 
        CancellationToken cancellationToken)
    {
        var calendarEvent = await _unitOfWork.CalendarEvents
            .GetByIdWithDetailsAsync(request.Id);

        if (calendarEvent == null)
        {
            throw new NotFoundException("Calendar event", request.Id);
        }

        if (calendarEvent.UserId != request.UserId)
        {
            throw new Exceptions.UnauthorizedAccessException("You are not authorized to view this event");
        }

        return new CalendarEventItemDto
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
            OutfitName = calendarEvent.WearEvent?.Outfit?.Name,
            OutfitImageUrl = calendarEvent.WearEvent?.Outfit?.ImageUrl,
            Notes = calendarEvent.Notes,
            IsRecurring = calendarEvent.IsRecurring
        };
    }
}
