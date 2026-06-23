using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.DTOs.Wardrobe.Validators;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Features.Notifications.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using System.Linq;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;

public class RecordWearCommandHandler : IRequestHandler<RecordWearCommand, ClothingItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordWearCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public RecordWearCommandHandler(IUnitOfWork unitOfWork, ILogger<RecordWearCommandHandler> logger, IMapper mapper, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ClothingItemDto> Handle(RecordWearCommand request, CancellationToken cancellationToken)
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

            // Wear milestone notification (current month)
            var now = DateTimeOffset.UtcNow;
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
            var monthWears = await _unitOfWork.WearEvents.FindAsync(we =>
                we.UserId == request.UserId &&
                we.ClothingItemId == request.Request.ClothingItemId &&
                we.WornAt >= monthStart);
            var monthWearCount = monthWears.Count();

            var milestones = new[] { 10, 25, 50, 100 };
            if (milestones.Contains(monthWearCount))
            {
                await _mediator.Send(new CreateNotificationCommand
                {
                    UserId = request.UserId,
                    Request = new CreateNotificationDto
                    {
                        Type = NotificationType.System,
                        Title = "Wear Count Update",
                        Message = $"You've worn your \"{clothingItem.Name}\" {monthWearCount} times this month!",
                        ActionUrl = $"/wardrobe/{clothingItem.Id}"
                    }
                });
            }

            _logger.LogInformation("Recorded wear for clothing item {ClothingItemId}, new wear count: {WearCount}",
                request.Request.ClothingItemId, clothingItem.WearCount);

            return _mapper.Map<ClothingItemDto>(clothingItem);
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
