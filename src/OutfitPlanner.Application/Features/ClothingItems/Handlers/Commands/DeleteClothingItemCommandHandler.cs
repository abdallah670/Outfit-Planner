using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;

public class DeleteClothingItemCommandHandler : IRequestHandler<DeleteClothingItemCommand, BaseCommandResponse>
{
    private readonly ILogger<DeleteClothingItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    

    public DeleteClothingItemCommandHandler(ILogger<DeleteClothingItemCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    async Task<BaseCommandResponse> IRequestHandler<DeleteClothingItemCommand, BaseCommandResponse>.Handle(DeleteClothingItemCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            // Validate the request
            var validationResult = await new DeleteClothingItemCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for delete clothing item request for user with ID {UserId}", request.UserId);
                throw new OutfitPlanner.Application.Exceptions.ValidationException(validationResult);
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to delete clothing item", request.UserId);
                throw new NotFoundException("User", request.UserId);
            }

            // Check if clothing item exists and belongs to the user
            var clothingItem = await _unitOfWork.ClothingItems.GetByIdAsync(request.Id);
            if (clothingItem == null)
            {
                _logger.LogWarning("Clothing item with ID {ClothingItemId} not found", request.Id);
                throw new NotFoundException("Clothing item", request.Id);
            }

            if (clothingItem.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete clothing item {ClothingItemId} belonging to another user", request.UserId, request.Id);
                throw new Exceptions.UnauthorizedAccessException("You are not authorized to delete this clothing item");
            }

            // Delete the item
            await _unitOfWork.ClothingItems.RemoveAsync(clothingItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.Message = "Clothing item deleted successfully";
            response.Id = request.Id;
            return response;
        }
        catch (NotFoundException)
        {
            throw; // Re-throw NotFoundException to be handled by the API
        }
        catch (OutfitPlanner.Application.Exceptions.ValidationException)
        {
            throw; // Re-throw ValidationException to be handled by the API
        }
        catch (Exceptions.UnauthorizedAccessException)
        {
            throw; // Re-throw UnauthorizedAccessException to be handled by the API
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting clothing item with ID {ClothingItemId} for user with ID {UserId}", request.Id, request.UserId);
            throw new BadRequestException("An error occurred while deleting the clothing item. Please try again later.");
        }
    }
}
