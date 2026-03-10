using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Implementation of outfit image generation service
/// </summary>
public class OutfitImageGeneratorService : IOutfitImageGeneratorService
{
    private readonly IImageCombinationService _imageCombinationService;
    private readonly IOutfitImageCacheService _imageCacheService;
    private readonly OutfitImageCacheSettings _cacheSettings;
    private readonly ILogger<OutfitImageGeneratorService> _logger;
    private readonly string _webRootPath;

    public OutfitImageGeneratorService(
        IImageCombinationService imageCombinationService,
        IOutfitImageCacheService imageCacheService,
        IOptions<OutfitImageCacheSettings> cacheSettings,
        ILogger<OutfitImageGeneratorService> logger)
    {
        _imageCombinationService = imageCombinationService ?? throw new ArgumentNullException(nameof(imageCombinationService));
        _imageCacheService = imageCacheService ?? throw new ArgumentNullException(nameof(imageCacheService));
        _cacheSettings = cacheSettings?.Value ?? new OutfitImageCacheSettings();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _webRootPath = GetWebRootPath();
    }

    private static string GetWebRootPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "wwwroot"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "wwwroot"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OutfitPlanner", "wwwroot")
        };
        
        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
                return path;
        }
        
        return possiblePaths[0];
    }

    public async Task<string?> GenerateOutfitImageAsync(Outfit outfit)
    {
        return await GenerateImageInternalAsync(outfit, regenerate: false);
    }

    public async Task<string?> RegenerateOutfitImageAsync(Outfit outfit)
    {
        return await GenerateImageInternalAsync(outfit, regenerate: true);
    }

    private async Task<string?> GenerateImageInternalAsync(Outfit outfit, bool regenerate)
    {
        try
        {
            _logger.LogInformation(
                "{Action} outfit image for outfit {OutfitId}", 
                regenerate ? "Regenerating" : "Generating", 
                outfit.Id);

            // Get clothing items with their images
            var itemsWithImages = outfit.Items
                .Where(i => i.ClothingItem != null && !string.IsNullOrEmpty(i.ClothingItem.ImageUrl))
                .ToList();

            if (itemsWithImages.Count < 2)
            {
                _logger.LogInformation(
                    "Outfit {OutfitId} has less than 2 items with images, skipping image generation",
                    outfit.Id);
                return null;
            }

            // Extract URLs, types, and names
            var imageUrls = itemsWithImages.Select(i => i.ClothingItem!.ImageUrl).ToList();
            var clothingTypes = itemsWithImages.Select(i => i.ClothingItem!.Type).ToList();
            var clothingNames = itemsWithImages.Select(i => i.ClothingItem!.Name).ToList();

            var baseUrl = "http://localhost:5000";

            // Combine images
            var combinedImage = await _imageCombinationService.CombineImagesAsync(
                imageUrls,
                clothingTypes,
                clothingNames,
                _webRootPath,
                baseUrl);

            if (combinedImage != null)
            {
                // If regenerating, delete old cached image first
                if (regenerate)
                {
                    await _imageCacheService.DeleteCachedImageAsync(outfit.Id);
                }

                // Cache the generated image
                await _imageCacheService.CacheImageAsync(outfit.Id, combinedImage);
                
                // Return the URL path for saving to database
                var imageUrl = $"/uploads/outfit-images/outfit-{outfit.Id}.jpg";
                _logger.LogInformation(
                    "Successfully {action} outfit image for outfit {OutfitId}: {ImageUrl}",
                    regenerate ? "regenerated" : "generated",
                    outfit.Id,
                    imageUrl);
                return imageUrl;
            }
            
            _logger.LogWarning("Failed to generate outfit image for outfit {OutfitId}", outfit.Id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error {action} outfit image for outfit {OutfitId}",
                regenerate ? "regenerating" : "generating",
                outfit.Id);
            return null;
        }
    }

    public async Task DeleteOutfitImageAsync(Guid outfitId)
    {
        try
        {
            await _imageCacheService.DeleteCachedImageAsync(outfitId);
            _logger.LogInformation("Deleted outfit image for outfit {OutfitId}", outfitId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting outfit image for outfit {OutfitId}", outfitId);
        }
    }
}
