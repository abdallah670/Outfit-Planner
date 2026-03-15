using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

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

            // If no items with images, generate a placeholder
            if (itemsWithImages.Count == 0)
            {
                _logger.LogInformation(
                    "Outfit {OutfitId} has no items with images, generating placeholder",
                    outfit.Id);
                
                var placeholderImage = await GeneratePlaceholderImageAsync(outfit);
                
                if (placeholderImage != null)
                {
                    // If regenerating, delete old cached image first
                    if (regenerate)
                    {
                        await _imageCacheService.DeleteCachedImageAsync(outfit.Id);
                    }

                    // Cache the generated image
                    await _imageCacheService.CacheImageAsync(outfit.Id, placeholderImage);
                    
                    // Return the URL path for saving to database
                    var imageUrl = $"/uploads/outfit-images/outfit-{outfit.Id}.jpg";
                    _logger.LogInformation(
                        "Generated placeholder outfit image for outfit {OutfitId}: {ImageUrl}",
                        outfit.Id,
                        imageUrl);
                    return imageUrl;
                }
                
                return null;
            }

            // If only 1 item with image, use that single image (scaled)
            if (itemsWithImages.Count == 1)
            {
                _logger.LogInformation(
                    "Outfit {OutfitId} has only 1 item with image, using single image",
                    outfit.Id);
                
                var singleImage = await GenerateSingleItemImageAsync(itemsWithImages.First());
                
                if (singleImage != null)
                {
                    // If regenerating, delete old cached image first
                    if (regenerate)
                    {
                        await _imageCacheService.DeleteCachedImageAsync(outfit.Id);
                    }

                    // Cache the generated image
                    await _imageCacheService.CacheImageAsync(outfit.Id, singleImage);
                    
                    // Return the URL path for saving to database
                    var imageUrl = $"/uploads/outfit-images/outfit-{outfit.Id}.jpg";
                    _logger.LogInformation(
                        "Generated single-item outfit image for outfit {OutfitId}: {ImageUrl}",
                        outfit.Id,
                        imageUrl);
                    return imageUrl;
                }
                
                return null;
            }

            // 2+ items with images - use existing combination logic

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

    /// <summary>
    /// Generates a placeholder image when no clothing items have images
    /// Creates a simple image with outfit name
    /// </summary>
    private async Task<byte[]?> GeneratePlaceholderImageAsync(Outfit outfit)
    {
        try
        {
            using var image = new Image<Rgba32>(400, 300);
            
            // Fill with a neutral gray background
            image.Mutate(ctx => ctx.BackgroundColor(Color.FromRgb(240, 240, 240)));
            
            // Get outfit name or use default
            var outfitName = !string.IsNullOrWhiteSpace(outfit.Name) ? outfit.Name : "Outfit";
            var itemCount = outfit.Items.Count;
            
            // Create a simple solid color image as placeholder
            // Fill center with a slightly darker gray to create visual interest
            for (int y = 100; y < 200; y++)
            {
                for (int x = 100; x < 300; x++)
                {
                    image[x, y] = Color.FromRgb(220, 220, 220);
                }
            }
            
            // Save to byte array
            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 });
            
            _logger.LogInformation("Generated placeholder image for outfit {OutfitId}", outfit.Id);
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating placeholder image for outfit {OutfitId}", outfit.Id);
            return null;
        }
    }

    /// <summary>
    /// Generates an image from a single clothing item
    /// Scales the single image to fit the outfit image dimensions
    /// </summary>
    private async Task<byte[]?> GenerateSingleItemImageAsync(OutfitItem outfitItem)
    {
        try
        {
            if (outfitItem.ClothingItem == null || string.IsNullOrEmpty(outfitItem.ClothingItem.ImageUrl))
            {
                return null;
            }

            var imageUrl = outfitItem.ClothingItem.ImageUrl;
            var clothingType = outfitItem.ClothingItem.Type;
            var clothingName = outfitItem.ClothingItem.Name ?? "Item";

            // Download the image
            byte[]? imageData = await DownloadImageAsync(imageUrl);
            
            if (imageData == null)
            {
                _logger.LogWarning("Could not download image for single item: {ImageUrl}", imageUrl);
                return null;
            }

            // Load and resize to standard outfit image size
            using var inputStream = new MemoryStream(imageData);
            using var image = await Image.LoadAsync<Rgba32>(inputStream);
            
            // Resize to standard outfit image dimensions (maintaining aspect ratio)
            const int targetWidth = 400;
            const int targetHeight = 300;
            
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(targetWidth, targetHeight),
                Mode = ResizeMode.Max
            }));

            // Save to byte array
            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 });
            
            _logger.LogInformation("Generated single-item image for clothing item {ItemId}", outfitItem.ClothingItemId);
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating single-item image for outfit item {ItemId}", outfitItem.Id);
            return null;
        }
    }

    /// <summary>
    /// Downloads an image from URL or local path
    /// </summary>
    private async Task<byte[]?> DownloadImageAsync(string url)
    {
        try
        {
            // Try local path first
            if (url.StartsWith("/uploads") || url.StartsWith("uploads"))
            {
                var relativePath = url.TrimStart('/');
                var fullPath = Path.Combine(_webRootPath, relativePath);
                
                if (File.Exists(fullPath))
                {
                    return await File.ReadAllBytesAsync(fullPath);
                }
                
                // Try wwwroot path
                var wwwrootPath = Path.Combine(_webRootPath, "wwwroot", relativePath);
                if (File.Exists(wwwrootPath))
                {
                    return await File.ReadAllBytesAsync(wwwrootPath);
                }
            }
            
            // Try HTTP URL
            if (url.StartsWith("http"))
            {
                using var client = new HttpClient();
                return await client.GetByteArrayAsync(url);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image from {Url}", url);
        }
        
        return null;
    }
}
