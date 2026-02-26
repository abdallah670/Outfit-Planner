using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Commands;

public class UpdateClothingItemCommandHandler : IRequestHandler<UpdateClothingItemCommand, ClothingItemDto>
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClothingItemCommandHandler> _logger;
        private readonly IMapper _mapper;

        public UpdateClothingItemCommandHandler(IUnitOfWork unitOfWork,
         ILogger<UpdateClothingItemCommandHandler> logger, IMapper mapper)
         {
             _mapper = mapper;
             _unitOfWork = unitOfWork;
             _logger = logger;
         }
         
    public async Task<ClothingItemDto> Handle(UpdateClothingItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateClothingItemCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation failed for update clothing item request for user with ID {UserId}. Errors: {Errors}", request.UserId, errors);
            throw new OutfitPlanner.Application.Exceptions.ValidationException(validationResult);
        }

        var clothingItem = await _unitOfWork.ClothingItems.GetByIdAsync(request.Id);
        if (clothingItem == null)
        {
            _logger.LogWarning("Clothing item with ID {ClothingItemId} not found", request.Id);
            throw new OutfitPlanner.Application.Exceptions.NotFoundException("Clothing item", request.Id);
        }
        if (clothingItem.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to update clothing item {ClothingItemId} belonging to another user", request.UserId, request.Id);
            throw new Exceptions.UnauthorizedAccessException("You are not authorized to update this clothing item");
        }

        _mapper.Map(request.Request, clothingItem);
        
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ClothingItemDto>(clothingItem);
        
    }
}
