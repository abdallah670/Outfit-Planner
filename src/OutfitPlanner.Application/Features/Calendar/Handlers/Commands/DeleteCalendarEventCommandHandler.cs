using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

/// <summary>
/// Handler for deleting a calendar event (time-based event)
/// Also deletes associated WearEvent if one exists
/// </summary>
public class DeleteCalendarEventCommandHandler : IRequestHandler<DeleteCalendarEventCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeleteCalendarEventCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(DeleteCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        // Get the CalendarEvent (not WearEvent!)
        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(request.Id);
        if (calendarEvent == null)
        {
            throw new NotFoundException("Calendar event not found", request.Id);
        }

        if (calendarEvent.UserId != request.UserId)
        {
            throw new Exceptions.UnauthorizedAccessException("You do not have permission to delete this event");
        }

        // If there's an associated WearEvent, delete it first
        if (calendarEvent.WearEventId.HasValue)
        {
            var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(calendarEvent.WearEventId.Value);
            if (wearEvent != null)
            {
                await _unitOfWork.WearEvents.RemoveAsync(wearEvent);
            }
        }

        // Delete the CalendarEvent
        await _unitOfWork.CalendarEvents.RemoveAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Calendar event deleted successfully";
        response.Id = calendarEvent.Id;

        return response;
    }
}
