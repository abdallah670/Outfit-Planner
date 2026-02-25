using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.DTOs.Wardrobe.Validators;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;

public class RecordWearCommandHandler : IRequestHandler<RecordWearCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordWearCommandHandler> _logger;

    public RecordWearCommandHandler(IUnitOfWork unitOfWork, ILogger<RecordWearCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(RecordWearCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            var validator = new RecordWearRequestValidator();
            var validationResult = await validator.ValidateAsync(request.Request, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }

            // Get the clothing item
            var clothingItem = await _unitOfWork.ClothingItems.GetByIdAsync(request.Request.ClothingItemId);
            if (clothingItem == null)
            {
                _logger.LogWarning("Clothing item with ID {ClothingItemId} not found", request.Request.ClothingItemId);
                throw new NotFoundException("Clothing item", request.Request.ClothingItemId);
            }

            // Check authorization
            if (clothingItem.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to record wear for clothing item {ClothingItemId} belonging to another user", 
                    request.UserId, request.Request.ClothingItemId);
                throw new Exceptions.UnauthorizedAccessException("You are not authorized to record wear for this clothing item");
            }

            // Update wear statistics on the clothing item
            clothingItem.WearCount++;
            clothingItem.LastWorn = request.Request.WornAt;

            // Create a new wear event record
            var wearEvent = new WearEvent
            {
                UserId = request.UserId,
                ClothingItemId = request.Request.ClothingItemId,
                WornAt = request.Request.WornAt,
                DurationMinutes = request.Request.DurationMinutes ?? 0,
                WeatherCondition = request.Request.WeatherCondition ?? string.Empty,
                Rating = request.Request.Rating ?? 0,
                Notes = request.Request.Notes ?? string.Empty
            };

            await _unitOfWork.WearEvents.AddAsync(wearEvent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Recorded wear for clothing item {ClothingItemId}, new wear count: {WearCount}", 
                request.Request.ClothingItemId, clothingItem.WearCount);

            return new BaseCommandResponse
            {
                Success = true,
                Message = "Wear event recorded successfully",
                Id = wearEvent.Id
            };
        }
        catch (NotFoundException)
        {
            throw; // Re-throw NotFoundException to preserve the correct error type
        }
        catch (Exceptions.UnauthorizedAccessException)
        {
            throw; // Re-throw UnauthorizedAccessException to preserve the correct error type
        }
        catch (ValidationException)
        {
            throw; // Re-throw ValidationException to preserve the correct error type
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while recording wear for clothing item {ClothingItemId}", 
                request.Request.ClothingItemId);
            throw new BadRequestException("An error occurred while recording the wear event. Please try again later.");
        }
    }
}
