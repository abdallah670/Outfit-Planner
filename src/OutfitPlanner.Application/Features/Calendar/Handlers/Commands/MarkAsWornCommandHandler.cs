using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

public class MarkAsWornCommandHandler : IRequestHandler<MarkAsWornCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MarkAsWornCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(MarkAsWornCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(request.Id);
        if (wearEvent == null)
        {
            throw new NotFoundException("Event not found", request.Id);
        }

        if (wearEvent.UserId != request.UserId)
        {
            throw new Exceptions.UnauthorizedAccessException("You do not have permission to update this event");
        }

        // Update the event with wear details
        wearEvent.DurationMinutes = request.Request.DurationMinutes;
        wearEvent.WeatherCondition = request.Request.WeatherCondition ?? string.Empty;
        wearEvent.Rating = request.Request.Rating;
        wearEvent.Notes = request.Request.Notes ?? wearEvent.Notes;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Marked as worn successfully";
        response.Id = wearEvent.Id;

        return response;
    }
}
