using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

public class RecordOutfitWearCommandHandler : IRequestHandler<RecordOutfitWearCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RecordOutfitWearCommandHandler> _logger;

    public RecordOutfitWearCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RecordOutfitWearCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(RecordOutfitWearCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var outfit = await _unitOfWork.Outfits.GetWithItemsByIdAsync(request.OutfitId);
            if (outfit == null)
            {
                return new BaseCommandResponse
                {
                    Success = false,
                    Message = "Outfit not found",
                    Errors = new List<string> { "Outfit not found" }
                };
            }

            var wearEvent = new WearEvent
            {
                OutfitId = outfit.Id,
                UserId = request.UserId,
                WornAt = request.WornAt,
                WeatherCondition = request.WeatherCondition ?? string.Empty,
                EventId = request.EventId
            };
            await _unitOfWork.WearEvents.AddAsync(wearEvent);

            // Update Outfit counters
            outfit.TimesWorn++;
            outfit.LastWorn = request.WornAt;

            // Cascade to Items
            foreach (var item in outfit.Items)
            {
                if (item.ClothingItem != null)
                {
                    item.ClothingItem.WearCount++;
                    item.ClothingItem.LastWorn = request.WornAt;

                    // Create individual wear event for each item for history tracking
                    var itemWearEvent = new WearEvent
                    {
                        ClothingItemId = item.ClothingItemId,
                        OutfitId = outfit.Id, 
                        UserId = request.UserId,
                        WornAt = request.WornAt,
                        WeatherCondition = request.WeatherCondition ?? string.Empty,
                        EventId = request.EventId
                    };
                    await _unitOfWork.WearEvents.AddAsync(itemWearEvent);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Recorded wear for outfit {OutfitId} and its items for user {UserId}",
                outfit.Id,
                request.UserId);

            return new BaseCommandResponse
            {
                Success = true,
                Message = "Outfit wear recorded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording outfit wear for user {UserId}", request.UserId);
            throw new BadRequestException("Error recording outfit wear");
        }
    }
}
