using AutoMapper;
using FluentValidation;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Exceptions;
using EntityValidationException = OutfitPlanner.Application.Exceptions.ValidationException;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

/// <summary>
/// Handler for creating a new outfit with validation, item ownership checks,
/// and pre-generation of outfit images.
/// </summary>
public class CreateOutfitCommandHandler : IRequestHandler<CreateOutfitCommand, OutfitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateOutfitCommand> _validator;
    private readonly ILogger<CreateOutfitCommandHandler> _logger;
    private readonly IImageCombinationService _imageCombinationService;
    private readonly IOutfitImageCacheService _imageCacheService;
    private readonly OutfitImageCacheSettings _cacheSettings;

    public CreateOutfitCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateOutfitCommand> validator,
        ILogger<CreateOutfitCommandHandler> logger,
        IImageCombinationService imageCombinationService,
        IOutfitImageCacheService imageCacheService,
        IOptions<OutfitImageCacheSettings> cacheSettings)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _imageCombinationService = imageCombinationService ?? throw new ArgumentNullException(nameof(imageCombinationService));
        _imageCacheService = imageCacheService ?? throw new ArgumentNullException(nameof(imageCacheService));
        _cacheSettings = cacheSettings?.Value ?? new OutfitImageCacheSettings();
    }

    public async Task<OutfitDto> Handle(CreateOutfitCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        try
        {
            // Validate command using FluentValidation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new EntityValidationException(validationResult);
            }

            // Validate all clothing items exist and belong to the user (single database query)
            await ValidateClothingItemsAsync(request, cancellationToken);

            // Create the outfit entity
            var outfit = _mapper.Map<Outfit>(request.Request);
            outfit.UserId = request.UserId;
            outfit.Status = OutfitStatus.Active;
            outfit.CreatedAt = DateTimeOffset.UtcNow;

            // Clear items mapped from DTO and add properly constructed items
            outfit.Items.Clear();
            foreach (var itemDto in request.Request.Items)
            {
                var outfitItem = new OutfitItem
                {
                    ClothingItemId = itemDto.ClothingItemId,
                    Role = Enum.TryParse<ItemRole>(itemDto.Role, true, out var role) ? role : ItemRole.Primary,
                    LayeringOrder = itemDto.LayeringOrder,
                    IsEssential = itemDto.IsEssential
                };
                outfit.Items.Add(outfitItem);
            }

            await _unitOfWork.Outfits.AddAsync(outfit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Fetch again with items to ensure return DTO is fully populated
            var savedOutfit = await _unitOfWork.Outfits.GetWithItemsByIdAsync(outfit.Id);

            // Pre-generate outfit image if enabled and outfit has multiple items
            if (_cacheSettings.EnablePreGeneration && savedOutfit.Items.Count > 1)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await PreGenerateOutfitImageAsync(savedOutfit);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to pre-generate outfit image for outfit {OutfitId}", outfit.Id);
                    }
                }, cancellationToken);
            }

            _logger.LogInformation(
                "Created outfit {OutfitId} for user {UserId} with {ItemCount} items",
                outfit.Id,
                request.UserId,
                outfit.Items.Count);

            return _mapper.Map<OutfitDto>(savedOutfit);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Operation cancelled while creating outfit for user {UserId}",
                request.UserId);
            throw;
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(
                ex,
                "Error creating outfit for user {UserId}",
                request.UserId);
            throw new BadRequestException($"Error creating outfit: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that all clothing items exist and belong to the requesting user.
    /// Uses a single database query for efficiency.
    /// </summary>
    private async Task ValidateClothingItemsAsync(
        CreateOutfitCommand request,
        CancellationToken cancellationToken)
    {
        var itemIds = request.Request.Items
            .Select(i => i.ClothingItemId)
            .Distinct()
            .ToList();

        if (!itemIds.Any())
        {
            throw new BadRequestException("At least one clothing item is required");
        }

        // Single database query to get all items by IDs AND verify ownership
        var existingItems = await _unitOfWork.ClothingItems
            .FindAsync(x => itemIds.Contains(x.Id) && x.UserId == request.UserId)
            .ConfigureAwait(false);

        var existingIds = existingItems.Select(x => x.Id).ToHashSet();
        var missingIds = itemIds.Where(id => !existingIds.Contains(id)).ToList();

        if (missingIds.Any())
        {
            _logger.LogWarning(
                "User {UserId} attempted to create outfit with invalid clothing items: {MissingIds}",
                request.UserId,
                string.Join(", ", missingIds));

            throw new BadRequestException("Some clothing items were not found or don't belong to you");
        }
    }

    /// <summary>
    /// Pre-generates and caches the outfit image in the background
    /// </summary>
    private async Task PreGenerateOutfitImageAsync(Outfit outfit)
    {
        try
        {
            _logger.LogInformation("Pre-generating outfit image for outfit {OutfitId}", outfit.Id);

            // Get clothing items with their images
            var itemsWithImages = outfit.Items
                .Where(i => !string.IsNullOrEmpty(i.ClothingItem.ImageUrl))
                .ToList();

            if (itemsWithImages.Count < 2)
            {
                _logger.LogInformation("Outfit {OutfitId} has less than 2 items with images, skipping pre-generation", outfit.Id);
                return;
            }

            // Extract URLs, types, and names
            var imageUrls = itemsWithImages.Select(i => i.ClothingItem.ImageUrl).ToList();
            var clothingTypes = itemsWithImages.Select(i => i.ClothingItem.Type).ToList();
            var clothingNames = itemsWithImages.Select(i => i.ClothingItem.Name).ToList();

            // Generate combined image
            var combinedImage = await _imageCombinationService.CombineImagesFromPathsAsync(
                imageUrls,
                clothingTypes,
                clothingNames);

            if (combinedImage != null)
            {
                // Cache the generated image
                await _imageCacheService.CacheImageAsync(outfit.Id, combinedImage);
                _logger.LogInformation("Successfully pre-generated and cached outfit image for outfit {OutfitId}", outfit.Id);
            }
            else
            {
                _logger.LogWarning("Failed to generate outfit image for outfit {OutfitId}", outfit.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pre-generating outfit image for outfit {OutfitId}", outfit.Id);
        }
    }
}
