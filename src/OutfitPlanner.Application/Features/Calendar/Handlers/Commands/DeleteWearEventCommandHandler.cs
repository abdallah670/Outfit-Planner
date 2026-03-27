using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

/// <summary>
/// Handler for deleting a WearEvent (scheduled outfit)
/// </summary>
public class DeleteWearEventCommandHandler : IRequestHandler<DeleteWearEventCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWearEventCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseCommandResponse> Handle(DeleteWearEventCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(request.Id);
        if (wearEvent == null)
        {
            throw new NotFoundException("Wear event not found", request.Id);
        }

        if (wearEvent.UserId != request.UserId)
        {
            throw new Exceptions.UnauthorizedAccessException("You do not have permission to delete this event");
        }

        await _unitOfWork.WearEvents.RemoveAsync(wearEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Wear event deleted successfully";
        response.Id = wearEvent.Id;

        return response;
    }
}
