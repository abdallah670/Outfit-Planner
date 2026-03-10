using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Queries;

/// <summary>
/// Get calendar events for a specific date (for sidebar display)
/// </summary>
public class GetCalendarEventsByDateRequest : IRequest<List<CalendarEventItemDto>>
{
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
}

/// <summary>
/// Get calendar events for a month
/// </summary>
public class GetCalendarEventsForMonthRequest : IRequest<List<CalendarEventItemDto>>
{
    public string UserId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
}
