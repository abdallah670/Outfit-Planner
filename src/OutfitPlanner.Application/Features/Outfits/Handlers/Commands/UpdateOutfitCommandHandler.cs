using AutoMapper;
using FluentValidation;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Domain.Entities;
using ValidationException = OutfitPlanner.Application.Exceptions.ValidationException;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

public class UpdateOutfitCommandHandler : IRequestHandler<UpdateOutfitCommand, OutfitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateOutfitCommand> _validator;
    private readonly ILogger<UpdateOutfitCommandHandler> _logger;

    public UpdateOutfitCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<UpdateOutfitCommand> validator,
        ILogger<UpdateOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    private async Task ValidateClothingItemsAsync(
        UpdateOutfitCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Request.Items == null || !request.Request.Items.Any()) return;

        var itemIds = request.Request.Items
            .Select(i => i.ClothingItemId)
            .Distinct()
            .ToList();

        // Single database query to get all items by IDs AND verify ownership
        var existingItems = await _unitOfWork.ClothingItems
            .FindAsync(x => itemIds.Contains(x.Id) && x.UserId == request.UserId)
            .ConfigureAwait(false);

        var existingIds = existingItems.Select(x => x.Id).ToHashSet();
        var missingIds = itemIds.Where(id => !existingIds.Contains(id)).ToList();

        if (missingIds.Any())
        {
            _logger.LogWarning(
                "User {UserId} attempted to update outfit {OutfitId} with invalid clothing items: {MissingIds}",
                request.UserId,
                request.Id,
                string.Join(", ", missingIds));

            throw new BadRequestException("Some clothing items were not found or don't belong to you");
        }
    }

    public async Task<OutfitDto> Handle(UpdateOutfitCommand request, CancellationToken cancellationToken)
    {
        try{
            var validationResult=await _validator.ValidateAsync(request, cancellationToken);
            if(!validationResult.IsValid){
                throw new ValidationException(validationResult);
            }
            var outfit = await _unitOfWork.Outfits.GetWithItemsByIdAsync(request.Id);
            if(outfit==null){
                throw new NotFoundException(nameof(Outfit), request.Id);
            }

            // Verify ownership
            if (outfit.UserId != request.UserId) {
                throw new BadRequestException("You do not own this outfit");
            }

            // Validate new items exist and belong to user
            await ValidateClothingItemsAsync(request, cancellationToken);

            _mapper.Map(request.Request, outfit);

            // Manual sync of items
            if (request.Request.Items != null)
            {
                outfit.Items.Clear();
                foreach (var itemDto in request.Request.Items)
                {
                    outfit.Items.Add(new OutfitItem
                    {
                        OutfitId = outfit.Id,
                        ClothingItemId = itemDto.ClothingItemId,
                        Role = Enum.TryParse<ItemRole>(itemDto.Role, true, out var role) ? role : ItemRole.Primary,
                        LayeringOrder = itemDto.LayeringOrder,
                        IsEssential = itemDto.IsEssential
                    });
                }
            }

            await _unitOfWork.Outfits.UpdateAsync(outfit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Fetch again to ensure items are populated for the return DTO
            var updatedOutfit = await _unitOfWork.Outfits.GetWithItemsByIdAsync(outfit.Id);
            return _mapper.Map<OutfitDto>(updatedOutfit);
        }
        catch(Exception ex){
            _logger.LogError(ex, "Error updating outfit {Id}", request.Id);
            throw;
        }
      
    }
}
