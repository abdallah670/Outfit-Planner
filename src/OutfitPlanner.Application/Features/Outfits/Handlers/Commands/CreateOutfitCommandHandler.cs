using AutoMapper;
using FluentValidation;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Exceptions;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

/// <summary>
/// Handler for creating a new outfit with validation and item ownership checks.
/// </summary>
public class CreateOutfitCommandHandler : IRequestHandler<CreateOutfitCommand, BaseCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateOutfitCommand> _validator;
    private readonly ILogger<CreateOutfitCommandHandler> _logger;

    public CreateOutfitCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateOutfitCommand> validator,
        ILogger<CreateOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BaseCommandResponse> Handle(CreateOutfitCommand request, CancellationToken cancellationToken)
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
                return new BaseCommandResponse
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
            }

            // Validate all clothing items exist and belong to the user (single database query)
            var validationResponse = await ValidateClothingItemsAsync(request, cancellationToken);
            if (validationResponse != null)
            {
                return validationResponse;
            }

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

            _logger.LogInformation(
                "Created outfit {OutfitId} for user {UserId} with {ItemCount} items",
                outfit.Id,
                request.UserId,
                outfit.Items.Count);

            return new BaseCommandResponse
            {
                Success = true,
                Message = "Outfit created successfully",
                Id = outfit.Id
            };
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
    private async Task<BaseCommandResponse?> ValidateClothingItemsAsync(
        CreateOutfitCommand request,
        CancellationToken cancellationToken)
    {
        var itemIds = request.Request.Items
            .Select(i => i.ClothingItemId)
            .Distinct()
            .ToList();

        if (!itemIds.Any())
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "At least one clothing item is required",
                Errors = new List<string> { "Items list cannot be empty" }
            };
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

            return new BaseCommandResponse
            {
                Success = false,
                Message = "Some clothing items were not found or don't belong to you",
                Errors = missingIds.Select(id => $"Clothing item {id} not found").ToList()
            };
        }

        return null;
    }
}
