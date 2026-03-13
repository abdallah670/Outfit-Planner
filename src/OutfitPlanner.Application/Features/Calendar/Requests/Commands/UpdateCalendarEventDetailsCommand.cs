using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Commands;

/// <summary>
/// Update CalendarEvent entity properties (Title, Description, Location, etc.)
/// Different from UpdateCalendarEventCommand which updates WearEvent properties
/// </summary>
public class UpdateCalendarEventDetailsCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UpdateCalendarEventItemRequest Request { get; set; } = new();
}
