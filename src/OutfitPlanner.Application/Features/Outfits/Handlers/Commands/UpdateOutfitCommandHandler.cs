using AutoMapper;
using FluentValidation;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
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
        _logger.LogInformation("UpdateOutfit START - OutfitId={Id}, UserId={UserId}", request.Id, request.UserId);

        // 1. Validate the command
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult);
        }

        // 2. Load the outfit WITHOUT items to keep the change tracker clean
        var outfit = await _unitOfWork.Outfits.GetByIdAsync(request.Id);
        if (outfit == null)
        {
            throw new NotFoundException(nameof(Outfit), request.Id);
        }

        // 3. Verify ownership
        if (outfit.UserId != request.UserId)
        {
            throw new BadRequestException("You do not own this outfit");
        }

        // 4. Validate new items exist and belong to user
        await ValidateClothingItemsAsync(request, cancellationToken);

        // 5. Map scalar properties
        _mapper.Map(request.Request, outfit);

        // 6. Handle items using ExecuteDeleteAsync — runs DELETE directly via SQL,
        //    completely bypassing the EF change tracker. This avoids the
        //    DbUpdateConcurrencyException that occurred with tracker-based approaches.
        if (request.Request.Items != null)
        {
            // Bulk delete all existing items for this outfit (pure SQL, no tracker)
            var deletedCount = await _unitOfWork.OutfitItems.DeleteByOutfitIdAsync(outfit.Id);
            _logger.LogInformation("Deleted {Count} existing items via ExecuteDelete", deletedCount);

            // Add new items
            foreach (var itemDto in request.Request.Items)
            {
                var role = Enum.TryParse<ItemRole>(itemDto.Role, true, out var r) ? r : ItemRole.Primary;

                await _unitOfWork.OutfitItems.AddAsync(new OutfitItem
                {
                    OutfitId = outfit.Id,
                    ClothingItemId = itemDto.ClothingItemId,
                    Role = role,
                    LayeringOrder = itemDto.LayeringOrder,
                    IsEssential = itemDto.IsEssential
                });
            }
        }

        // 7. Save — only the outfit UPDATE and item INSERTs go through the tracker
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UpdateOutfit SAVED successfully");

        // 8. Reload with full includes for the response DTO
        var updatedOutfit = await _unitOfWork.Outfits.GetWithItemsByIdAsync(outfit.Id);
        return _mapper.Map<OutfitDto>(updatedOutfit!);
    }
}
