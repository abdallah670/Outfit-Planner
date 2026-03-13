using MediatR;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Commands;

/// <summary>
/// Create a new calendar event
/// </summary>
public class CreateCalendarEventCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public CreateCalendarEventRequest Request { get; set; } = null!;
}
