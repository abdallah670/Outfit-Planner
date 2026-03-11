using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

public class UpdateCalendarEventCommandHandler : IRequestHandler<UpdateCalendarEventCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCalendarEventCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(UpdateCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        var wearEvent = await _unitOfWork.WearEvents.GetByIdAsync(request.Id);
        if (wearEvent == null)
        {
            throw new NotFoundException(nameof(WearEvent), request.Id);
        }

        if (wearEvent.UserId != request.UserId)
        {
            throw new Exceptions.UnauthorizedAccessException("You do not have permission to update this event");
        }

        if (request.Request.WornAt.HasValue)
            wearEvent.WornAt = request.Request.WornAt.Value;
        if (request.Request.DurationMinutes.HasValue)
            wearEvent.DurationMinutes = request.Request.DurationMinutes.Value;
        if (request.Request.WeatherCondition != null)
            wearEvent.WeatherCondition = request.Request.WeatherCondition;
        if (request.Request.Rating.HasValue)
            wearEvent.Rating = request.Request.Rating.Value;
        if (request.Request.Notes != null)
            wearEvent.Notes = request.Request.Notes;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Event updated successfully";
        response.Id = wearEvent.Id;

        return response;
    }
}
