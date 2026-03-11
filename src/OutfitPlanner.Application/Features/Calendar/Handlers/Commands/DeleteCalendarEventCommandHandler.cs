using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

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

        var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(request.Id);
        if (wearEvent == null)
        {
            throw new NotFoundException("Event not found", request.Id);
        }

        if (wearEvent.UserId != request.UserId)
        {
            throw new Exceptions.UnauthorizedAccessException("You do not have permission to delete this event");
        }

        await _unitOfWork.WearEvents.RemoveAsync(wearEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Event deleted successfully";
        response.Id = wearEvent.Id;

        return response;
    }
}
