using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;

public class CreateClothingItemCommandHandler : IRequestHandler<CreateClothingItemCommand, BaseCommandResponse>
{
    private readonly ILogger<CreateClothingItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateClothingItemCommandHandler(ILogger<CreateClothingItemCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    async Task<BaseCommandResponse> IRequestHandler<CreateClothingItemCommand, BaseCommandResponse>.Handle(CreateClothingItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = new BaseCommandResponse();
            var validationResult = await new CreateClothingItemCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for clothing item creation request for user with ID {UserId}", request.UserId);
                throw new OutfitPlanner.Application.Exceptions.ValidationException(validationResult);
            }
            var clothingItem = new ClothingItem
            {
                UserId = request.UserId,
                Name = request.Request.Name,
                Category = request.Request.Category,
                PrimaryColor = request.Request.PrimaryColor,
                ImageUrl = request.Request.ImageUrl,
                Type = Enum.Parse<ClothingType>(request.Request.Type),
                ThumbnailUrl = request.Request.ThumbnailUrl
            };
            await _unitOfWork.ClothingItems.AddAsync(clothingItem);
            await _unitOfWork.SaveChangesAsync();
            response.Success = true;
            response.Message = "Clothing item created successfully";
            response.Id = clothingItem.Id;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new clothing item for user with ID {UserId}", request.UserId);
            throw new BadRequestException("An error occurred while creating the clothing item. Please try again later.");
        }
    }
}
