using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Services;

/// <summary>
/// Enhanced image combination service using AI guide principles
/// </summary>
public class ImageCombinationService : IImageCombinationService
{
    private readonly HttpClient _httpClient;
    private readonly IOutfitImageProcessingService _processingService;
    private readonly OutfitLayoutConfig _layoutConfig;
    private readonly ILogger<ImageCombinationService>? _logger;

    public ImageCombinationService(
        IOutfitImageProcessingService? processingService = null,
        ILogger<ImageCombinationService>? logger = null)
    {
        _httpClient = new HttpClient();
        _processingService = processingService ?? new OutfitImageProcessingService();
        _layoutConfig = new OutfitLayoutConfig();
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<byte[]?> CombineImagesAsync(
        IEnumerable<string> imageUrls,
        IEnumerable<ClothingType> clothingTypes,
        IEnumerable<string> clothingNames,
        string webRootPath,
        string baseUrl)
    {
        try
        {
            var urls = imageUrls.ToList();
            var types = clothingTypes.ToList();
            var names = clothingNames.ToList();

            if (urls.Count == 0 || types.Count == 0 || names.Count == 0)
            {
                _logger?.LogWarning("No images provided for combination");
                return null;
            }

            // Download images and prepare streams
            var imageStreams = new List<(Stream Stream, ClothingType Type, string Name)>();
            
            for (int i = 0; i < urls.Count; i++)
            {
                var url = urls[i];
                var type = i < types.Count ? types[i] : ClothingType.Top;
                var name = i < names.Count ? names[i] : $"Item {i + 1}";

                byte[]? imageData = await DownloadImageAsync(url, webRootPath, baseUrl);
                
                if (imageData != null)
                {
                    var stream = new MemoryStream(imageData);
                    imageStreams.Add((stream, type, name));
                }
            }

            if (imageStreams.Count == 0)
            {
                _logger?.LogWarning("No valid images could be loaded");
                return null;
            }

            // Process and combine images using the new pipeline
            var result = await ProcessAndCombineImagesAsync(imageStreams);
            
            // Dispose streams
            foreach (var (stream, _, _) in imageStreams)
            {
                await stream.DisposeAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error combining images with smart layout");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]?> CombineImagesFromPathsAsync(
        IEnumerable<string> imagePaths,
        IEnumerable<ClothingType> clothingTypes,
        IEnumerable<string> clothingNames)
    {
        try
        {
            var paths = imagePaths.ToList();
            var types = clothingTypes.ToList();
            var names = clothingNames.ToList();

            if (paths.Count == 0 || types.Count == 0 || names.Count == 0)
            {
                _logger?.LogWarning("No image paths provided for combination");
                return null;
            }

            // Load images from paths
            var imageStreams = new List<(Stream Stream, ClothingType Type, string Name)>();
            
            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                var type = i < types.Count ? types[i] : ClothingType.Top;
                var name = i < names.Count ? names[i] : $"Item {i + 1}";

                if (File.Exists(path))
                {
                    var stream = new MemoryStream(await File.ReadAllBytesAsync(path));
                    imageStreams.Add((stream, type, name));
                }
                else
                {
                    _logger?.LogWarning("Image file not found: {Path}", path);
                }
            }

            if (imageStreams.Count == 0)
            {
                _logger?.LogWarning("No valid images could be loaded from paths");
                return null;
            }

            // Process and combine images
            var result = await ProcessAndCombineImagesAsync(imageStreams);
            
            // Dispose streams
            foreach (var (stream, _, _) in imageStreams)
            {
                await stream.DisposeAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error combining images from paths");
            return null;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use CombineImagesAsync with clothing types for better results")]
    public async Task<byte[]?> CombineImagesAsync(IEnumerable<string> imageUrls, string webRootPath, string baseUrl)
    {
        // Fallback to simple vertical stacking for backward compatibility
        return await CombineImagesLegacyAsync(imageUrls, webRootPath, baseUrl);
    }

    /// <inheritdoc />
    [Obsolete("Use CombineImagesFromPathsAsync with clothing types for better results")]
    public async Task<byte[]?> CombineImagesFromPathsAsync(IEnumerable<string> imagePaths)
    {
        // Fallback to simple vertical stacking for backward compatibility
        return await CombineImagesFromPathsLegacyAsync(imagePaths);
    }

    /// <summary>
    /// Main processing pipeline following AI guide principles
    /// </summary>
    private async Task<byte[]> ProcessAndCombineImagesAsync(
        List<(Stream Stream, ClothingType Type, string Name)> imageStreams)
    {
        // Process all items with the new pipeline
        var processedItems = await _processingService.ProcessOutfitItemsAsync(
            imageStreams.Select(x => (x.Stream, x.Type, x.Name)).ToList());

        // Calculate final canvas dimensions
        int maxContentWidth = 0;
        int totalContentHeight = _layoutConfig.Padding;
        
        foreach (var tuple in processedItems)
        {
            var contentBounds = _processingService.DetectBoundingBox(tuple.Item.Image);
            maxContentWidth = Math.Max(maxContentWidth, contentBounds.Width);
            totalContentHeight += contentBounds.Height + _layoutConfig.VerticalSpacing;
        }
        
        totalContentHeight += _layoutConfig.Padding - _layoutConfig.VerticalSpacing;

        // Create final composition canvas
        int canvasWidth = Math.Max(_layoutConfig.CanvasWidth, maxContentWidth + _layoutConfig.Padding * 2);
        int canvasHeight = Math.Max(_layoutConfig.CanvasHeight, totalContentHeight);
        
        using var finalCanvas = new Image<Rgba32>(canvasWidth, canvasHeight);
        
        // Fill with white background
        finalCanvas.Mutate(ctx => ctx.BackgroundColor(Color.White));

        // Draw each processed item at its calculated position
        foreach (var tuple in processedItems)
        {
            var item = tuple.Item;
            var position = tuple.Position;
            
            // Get the actual content bounds to draw only the non-transparent part
            var contentBounds = _processingService.DetectBoundingBox(item.Image);
            
            // Calculate centered X position
            int drawX = (canvasWidth - contentBounds.Width) / 2;
            
            // Crop to just the content for drawing
            using var contentImage = item.Image.Clone(ctx => ctx.Crop(contentBounds));
            
            // Draw the content image
            finalCanvas.Mutate(ctx => ctx.DrawImage(contentImage, new Point(drawX, position.Y), 1f));
        }

        // Save to byte array
        using var outputStream = new MemoryStream();
        await finalCanvas.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 90 });
        
        return outputStream.ToArray();
    }

    /// <summary>
    /// Downloads an image from URL or loads from local path
    /// </summary>
    private async Task<byte[]?> DownloadImageAsync(string url, string webRootPath, string baseUrl)
    {
        try
        {
            // Check if it's a local file path
            if (url.StartsWith("/uploads") || url.StartsWith("uploads"))
            {
                var relativePath = url.TrimStart('/');
                var fullPath = Path.Combine(webRootPath ?? "", relativePath);
                
                if (File.Exists(fullPath))
                {
                    return await File.ReadAllBytesAsync(fullPath);
                }
                
                // Try wwwroot path
                var wwwrootPath = Path.Combine(webRootPath ?? "", "wwwroot", relativePath);
                if (File.Exists(wwwrootPath))
                {
                    return await File.ReadAllBytesAsync(wwwrootPath);
                }
            }
            else if (url.StartsWith("http"))
            {
                return await _httpClient.GetByteArrayAsync(url);
            }
            else if (!url.Contains("."))
            {
                // Might be a filename without path - try to find in uploads
                var possiblePath = Path.Combine(webRootPath ?? "", "uploads", url);
                if (File.Exists(possiblePath))
                {
                    return await File.ReadAllBytesAsync(possiblePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to download/load image: {Url}", url);
        }

        return null;
    }

    /// <summary>
    /// Legacy simple vertical stacking method
    /// </summary>
    private async Task<byte[]?> CombineImagesLegacyAsync(IEnumerable<string> imageUrls, string webRootPath, string baseUrl)
    {
        try
        {
            var images = new List<byte[]>();
            
            foreach (var url in imageUrls)
            {
                if (string.IsNullOrWhiteSpace(url)) continue;
                
                byte[]? imageData = await DownloadImageAsync(url, webRootPath, baseUrl);
                if (imageData != null)
                {
                    images.Add(imageData);
                }
            }

            if (images.Count == 0) return null;

            return await CombineImagesByteArraysLegacyAsync(images);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in legacy image combination");
            return null;
        }
    }

    /// <summary>
    /// Legacy simple vertical stacking from paths
    /// </summary>
    private async Task<byte[]?> CombineImagesFromPathsLegacyAsync(IEnumerable<string> imagePaths)
    {
        try
        {
            var images = new List<byte[]>();
            
            foreach (var path in imagePaths)
            {
                if (File.Exists(path))
                {
                    var imageData = await File.ReadAllBytesAsync(path);
                    images.Add(imageData);
                }
            }

            if (images.Count == 0) return null;

            return await CombineImagesByteArraysLegacyAsync(images);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in legacy path combination");
            return null;
        }
    }

    /// <summary>
    /// Legacy byte array combination - simple vertical stacking
    /// </summary>
    private async Task<byte[]> CombineImagesByteArraysLegacyAsync(List<byte[]> images)
    {
        // Load all images
        var loadedImages = new List<Image<Rgba32>>();
        
        foreach (var imageData in images)
        {
            using var ms = new MemoryStream(imageData);
            var image = await Image.LoadAsync<Rgba32>(ms);
            loadedImages.Add(image);
        }

        // Calculate dimensions for combined image
        const int maxWidth = 400;
        int totalHeight = loadedImages.Sum(img => img.Height);
        
        // Create combined image
        using var combined = new Image<Rgba32>(maxWidth, totalHeight);
        
        // Fill with white background
        combined.Mutate(x => x.BackgroundColor(Color.White));
        
        // Draw each image vertically
        int yOffset = 0;
        
        foreach (var image in loadedImages)
        {
            // Resize image to fit width while maintaining aspect ratio
            int newHeight = (int)((double)image.Height * maxWidth / image.Width);
            using var resized = image.Clone(ctx => ctx.Resize(maxWidth, newHeight));
            
            // Draw image
            combined.Mutate(x => x.DrawImage(resized, new Point(0, yOffset), 1f));
            
            yOffset += newHeight;
        }

        // Dispose individual images
        foreach (var img in loadedImages)
        {
            img.Dispose();
        }

        // Save to byte array
        using var outputStream = new MemoryStream();
        await combined.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 90 });
        
        return outputStream.ToArray();
    }
}
