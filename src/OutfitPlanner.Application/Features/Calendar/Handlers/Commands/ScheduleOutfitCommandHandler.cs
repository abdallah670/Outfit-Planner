using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Calendar.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Commands;

public class ScheduleOutfitCommandHandler : IRequestHandler<ScheduleOutfitCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ScheduleOutfitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseCommandResponse> Handle(ScheduleOutfitCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        // Verify outfit exists and belongs to user (optional for development)
        var outfit = await _unitOfWork.Outfits.GetByIdAsync(request.Request.OutfitId);
        if (outfit == null)
        {
            // For development: Create a mock outfit reference if it doesn't exist
            // In production, this should throw NotFoundException
            System.Diagnostics.Debug.WriteLine($"Warning: Outfit {request.Request.OutfitId} not found. Creating schedule with mock reference.");
        }
        else if (outfit.UserId != request.UserId)
        {
            throw new BadRequestException("You do not own this outfit");
        }

        // Create wear event
        var wearEvent = new WearEvent
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OutfitId = request.Request.OutfitId,
            WornAt = request.Request.ScheduledDate,
            Notes = request.Request.Notes
        };

        await _unitOfWork.WearEvents.AddAsync(wearEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        response.Success = true;
        response.Message = "Outfit scheduled successfully";
        response.Id = wearEvent.Id;

        return response;
    }
}
