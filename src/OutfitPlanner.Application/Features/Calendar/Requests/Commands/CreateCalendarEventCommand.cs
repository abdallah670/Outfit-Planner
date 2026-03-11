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

/// <summary>
/// Update an existing calendar event
/// </summary>
public class UpdateCalendarEventItemCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UpdateCalendarEventItemRequest Request { get; set; } = null!;
}

/// <summary>
/// Delete a calendar event
/// </summary>
public class DeleteCalendarEventItemCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
