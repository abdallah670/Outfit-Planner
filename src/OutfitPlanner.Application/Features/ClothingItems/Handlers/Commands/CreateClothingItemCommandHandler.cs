using AutoMapper;
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

public class CreateClothingItemCommandHandler : IRequestHandler<CreateClothingItemCommand, ClothingItemDto>
{
    private readonly ILogger<CreateClothingItemCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateClothingItemCommandHandler(ILogger<CreateClothingItemCommandHandler> logger, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ClothingItemDto> Handle(CreateClothingItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await new CreateClothingItemCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for clothing item creation request for user with ID {UserId}. Errors: {Errors}", request.UserId, errors);
                throw new OutfitPlanner.Application.Exceptions.ValidationException(validationResult);
            }
            
            var clothingItem = _mapper.Map<ClothingItem>(request.Request);
            clothingItem.UserId = request.UserId;

            await _unitOfWork.ClothingItems.AddAsync(clothingItem);
            await _unitOfWork.SaveChangesAsync();
            
            return _mapper.Map<ClothingItemDto>(clothingItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new clothing item for user with ID {UserId}", request.UserId);
            throw new BadRequestException("An error occurred while creating the clothing item. Please try again later.");
        }
    }
}
