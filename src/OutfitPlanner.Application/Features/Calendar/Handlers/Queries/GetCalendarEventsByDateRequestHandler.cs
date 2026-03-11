using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;
using OutfitPlanner.Domain.Entities;
using DtoCalendarEventType = OutfitPlanner.Application.DTOs.Calendar.CalendarEventType;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Queries;

/// <summary>
/// Handler for getting calendar events by date
/// </summary>
public class GetCalendarEventsByDateRequestHandler 
    : IRequestHandler<GetCalendarEventsByDateRequest, List<CalendarEventItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalendarEventsByDateRequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CalendarEventItemDto>> Handle(
        GetCalendarEventsByDateRequest request, 
        CancellationToken cancellationToken)
    {
        var events = await _unitOfWork.CalendarEvents
            .GetByUserIdAndDateAsync(request.UserId, request.Date);

        return events.Select(e => MapToDto(e)).ToList();
    }

    private static CalendarEventItemDto MapToDto(CalendarEvent e)
    {
        return new CalendarEventItemDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Location = e.Location,
            EventDate = e.EventDate,
            StartTime = e.StartTime.HasValue 
                ? DateTime.Today.Add(e.StartTime.Value).ToString("h:mm tt") 
                : null,
            EndTime = e.EndTime.HasValue 
                ? DateTime.Today.Add(e.EndTime.Value).ToString("h:mm tt") 
                : null,
            EventType = (DtoCalendarEventType)e.EventType,
            WearEventId = e.WearEventId,
            Notes = e.Notes,
            IsRecurring = e.IsRecurring
        };
    }
}

/// <summary>
/// Handler for getting calendar events for a month
/// </summary>
public class GetCalendarEventsForMonthRequestHandler 
    : IRequestHandler<GetCalendarEventsForMonthRequest, List<CalendarEventItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalendarEventsForMonthRequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CalendarEventItemDto>> Handle(
        GetCalendarEventsForMonthRequest request, 
        CancellationToken cancellationToken)
    {
        var events = await _unitOfWork.CalendarEvents
            .GetByUserIdAndMonthAsync(request.UserId, request.Year, request.Month);

        return events.Select(e => MapToDto(e)).ToList();
    }

    private static CalendarEventItemDto MapToDto(CalendarEvent e)
    {
        return new CalendarEventItemDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Location = e.Location,
            EventDate = e.EventDate,
            StartTime = e.StartTime.HasValue 
                ? DateTime.Today.Add(e.StartTime.Value).ToString("h:mm tt") 
                : null,
            EndTime = e.EndTime.HasValue 
                ? DateTime.Today.Add(e.EndTime.Value).ToString("h:mm tt") 
                : null,
            EventType = (DtoCalendarEventType)e.EventType,
            WearEventId = e.WearEventId,
            Notes = e.Notes,
            IsRecurring = e.IsRecurring
        };
    }
}
