using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Queries;

/// <summary>
/// Handler for getting calendar events by date
/// </summary>
public class GetCalendarEventsByDateRequestHandler 
    : IRequestHandler<GetCalendarEventsByDateRequest, List<CalendarEventItemDto>>
{
    private readonly AppDbContext _context;

    public GetCalendarEventsByDateRequestHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CalendarEventItemDto>> Handle(
        GetCalendarEventsByDateRequest request, 
        CancellationToken cancellationToken)
    {
        var startOfDay = request.Date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var events = await _context.CalendarEvents
            .AsNoTracking()
            .Where(e => e.UserId == request.UserId)
            .Where(e => e.EventDate >= startOfDay && e.EventDate <= endOfDay)
            .Include(e => e.WearEvent)
                .ThenInclude(we => we!.Outfit)
            .OrderBy(e => e.StartTime)
            .Select(e => new CalendarEventItemDto
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
                EventType = (CalendarEventType)e.EventType,
                WearEventId = e.WearEventId,
                OutfitName = e.WearEvent != null && e.WearEvent.Outfit != null 
                    ? e.WearEvent.Outfit.Name 
                    : null,
                OutfitImageUrl = e.WearEvent != null && e.WearEvent.Outfit != null 
                    ? e.WearEvent.Outfit.ImageUrl 
                    : null,
                Notes = e.Notes,
                IsRecurring = e.IsRecurring
            })
            .ToListAsync(cancellationToken);

        return events;
    }
}

/// <summary>
/// Handler for getting calendar events for a month
/// </summary>
public class GetCalendarEventsForMonthRequestHandler 
    : IRequestHandler<GetCalendarEventsForMonthRequest, List<CalendarEventItemDto>>
{
    private readonly AppDbContext _context;

    public GetCalendarEventsForMonthRequestHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CalendarEventItemDto>> Handle(
        GetCalendarEventsForMonthRequest request, 
        CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var events = await _context.CalendarEvents
            .AsNoTracking()
            .Where(e => e.UserId == request.UserId)
            .Where(e => e.EventDate >= startOfMonth && e.EventDate <= endOfMonth)
            .Include(e => e.WearEvent)
                .ThenInclude(we => we!.Outfit)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime)
            .Select(e => new CalendarEventItemDto
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
                EventType = (CalendarEventType)e.EventType,
                WearEventId = e.WearEventId,
                OutfitName = e.WearEvent != null && e.WearEvent.Outfit != null 
                    ? e.WearEvent.Outfit.Name 
                    : null,
                OutfitImageUrl = e.WearEvent != null && e.WearEvent.Outfit != null 
                    ? e.WearEvent.Outfit.ImageUrl 
                    : null,
                Notes = e.Notes,
                IsRecurring = e.IsRecurring
            })
            .ToListAsync(cancellationToken);

        return events;
    }
}
