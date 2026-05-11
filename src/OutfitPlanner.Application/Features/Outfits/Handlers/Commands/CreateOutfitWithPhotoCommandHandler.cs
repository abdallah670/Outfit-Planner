using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Commands;

/// <summary>
/// Handler for creating a new outfit with a photo upload (no clothing items required)
/// </summary>
public class CreateOutfitWithPhotoCommandHandler : IRequestHandler<CreateOutfitWithPhotoCommand, CreateOutfitWithPhotoResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<CreateOutfitWithPhotoCommandHandler> _logger;

    public CreateOutfitWithPhotoCommandHandler(
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<CreateOutfitWithPhotoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateOutfitWithPhotoResponseDto> Handle(CreateOutfitWithPhotoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentNullException.ThrowIfNull(request.Photo);

        try
        {
            // Validate the photo
            if (request.Photo.Length == 0)
            {
                throw new BadRequestException("Photo is required");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(request.Photo.ContentType.ToLower()))
            {
                throw new BadRequestException("Invalid file type. Only JPEG, PNG, and WebP are allowed.");
            }

            // Upload the photo first
            string imageUrl;
            using (var stream = request.Photo.OpenReadStream())
            {
                var fileName = $"outfit-photo-{Guid.NewGuid()}.jpg";
                var uploadResult = await _imageStorageService.UploadImageAsync(
                    stream,
                    fileName,
                    request.UserId,
                    cancellationToken);

                if (!uploadResult.Success)
                {
                    throw new BadRequestException($"Failed to upload image: {uploadResult.ErrorMessage}");
                }

                imageUrl = uploadResult.OriginalPath ?? $"/uploads/outfit-images/{fileName}";
            }

            // Create the outfit entity
            var outfit = new Outfit
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = request.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                ImageUrl = imageUrl,
                Items = new List<OutfitItem>() // Empty list - no clothing items
            };

            // Parse optional enums
            if (!string.IsNullOrEmpty(request.Occasion) &&
                Enum.TryParse<OccasionType>(request.Occasion, true, out var occasion))
            {
                outfit.Occasion = occasion;
            }

            if (!string.IsNullOrEmpty(request.Season) &&
                Enum.TryParse<Season>(request.Season, true, out var season))
            {
                outfit.Season = season;
            }

            // Save outfit
            await _unitOfWork.Outfits.AddAsync(outfit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created outfit {OutfitId} with photo for user {UserId}",
                outfit.Id,
                request.UserId);

            return new CreateOutfitWithPhotoResponseDto
            {
                Id = outfit.Id,
                Name = outfit.Name,
                ImageUrl = outfit.ImageUrl,
                Occasion = outfit.Occasion.ToString(),
                CreatedAt = outfit.CreatedAt
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Operation cancelled while creating outfit with photo for user {UserId}",
                request.UserId);
            throw;
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(
                ex,
                "Error creating outfit with photo for user {UserId}",
                request.UserId);
            throw new BadRequestException($"Error creating outfit: {ex.Message}");
        }
    }
}
