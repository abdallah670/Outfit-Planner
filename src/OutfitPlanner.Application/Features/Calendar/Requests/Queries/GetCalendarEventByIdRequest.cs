using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Queries;

/// <summary>
/// Request to get a single calendar event by ID
/// </summary>
public class GetCalendarEventByIdRequest : IRequest<CalendarEventItemDto>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
