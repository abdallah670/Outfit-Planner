using System.IO;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Infrastructure.Services.Models;


namespace OutfitPlanner.Infrastructure.Services;

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
    /// Main processing pipeline with grid-based layout
    /// </summary>
    private async Task<byte[]> ProcessAndCombineImagesAsync(
        List<(Stream Stream, ClothingType Type, string Name)> imageStreams)
    {
        // Process all items with the new pipeline
        var processedItems = await _processingService.ProcessOutfitItemsAsync(
            imageStreams.Select(x => (x.Stream, x.Type, x.Name)).ToList());

        if (processedItems.Count == 0)
        {
            _logger?.LogWarning("No items to combine");
            return new byte[0];
        }

        // Calculate grid layout based on item count
        var layout = CalculateGridLayout(processedItems.Count);
        
        // Create canvas with appropriate dimensions
        using var finalCanvas = new Image<Rgba32>(layout.CanvasWidth, layout.CanvasHeight);
        finalCanvas.Mutate(ctx => ctx.BackgroundColor(Color.White));
        
        // Draw each item in its grid cell
        for (int i = 0; i < processedItems.Count; i++)
        {
            var cell = layout.Cells[i];
            var item = processedItems[i].Item;
            
            // Resize item to fit cell
            using var resizedItem = ResizeForCell(item.Image, cell.Width, cell.Height);
            
            // Calculate centered position within cell
            int drawX = cell.X + (cell.Width - resizedItem.Width) / 2;
            int drawY = cell.Y + (cell.Height - resizedItem.Height) / 2;
            
            // Draw on canvas
            finalCanvas.Mutate(ctx => ctx.DrawImage(resizedItem, new Point(drawX, drawY), 1f));
        }
        
        // Save and return
        using var outputStream = new MemoryStream();
        await finalCanvas.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 90 });
        return outputStream.ToArray();
    }

    /// <summary>
    /// Downloads an image from URL or loads from local path
    /// Tries multiple strategies to locate the image file
    /// </summary>
    private async Task<byte[]?> DownloadImageAsync(string url, string webRootPath, string baseUrl, string? clothingItemId = null, string? userId = null)
    {
        try
        {
            // Strategy 1: Direct local file path (full path provided)
            if (url.StartsWith("/uploads") || url.StartsWith("uploads"))
            {
                var result = await TryLoadFromLocalPath(url, webRootPath);
                if (result != null) return result;
            }
            
            // Strategy 2: HTTP URL
            if (url.StartsWith("http"))
            {
                try
                {
                    return await _httpClient.GetByteArrayAsync(url);
                }
                catch (HttpRequestException)
                {
                    // HTTP failed, try to extract filename and search locally
                    var filename = Path.GetFileName(url);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        var result = await TryFindFileByName(filename, webRootPath, clothingItemId, userId);
                        if (result != null) return result;
                    }
                }
            }
            
            // Strategy 3: Simple filename - try to find in uploads directory
            if (!url.Contains('/') && !url.Contains('\\'))
            {
                // It's a simple filename like "blouse-04.jpg"
                var result = await TryFindFileByName(url, webRootPath, clothingItemId, userId);
                if (result != null) return result;
            }
            
            // Strategy 4: Try as relative path
            if (!url.StartsWith('/'))
            {
                url = "/" + url;
            }
            var relativeResult = await TryLoadFromLocalPath(url, webRootPath);
            if (relativeResult != null) return relativeResult;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to download/load image: {Url}", url);
        }

        return null;
    }

    /// <summary>
    /// Tries to load an image from a local path
    /// </summary>
    private async Task<byte[]?> TryLoadFromLocalPath(string url, string? webRootPath)
    {
        if (string.IsNullOrEmpty(webRootPath)) return null;
        
        var relativePath = url.TrimStart('/');
        
        // Try direct path
        var fullPath = Path.Combine(webRootPath, relativePath);
        if (File.Exists(fullPath))
        {
            return await File.ReadAllBytesAsync(fullPath);
        }
        
        // Try wwwroot path
        var wwwrootPath = Path.Combine(webRootPath, "wwwroot", relativePath);
        if (File.Exists(wwwrootPath))
        {
            return await File.ReadAllBytesAsync(wwwrootPath);
        }
        
        return null;
    }

    /// <summary>
    /// Tries to find a file by name in the uploads directory
    /// Searches in patterns like: uploads/{userId}/{clothingItemId}/{filename}
    /// </summary>
    private async Task<byte[]?> TryFindFileByName(string filename, string? webRootPath, string? clothingItemId, string? userId)
    {
        if (string.IsNullOrEmpty(webRootPath)) return null;
        
        var uploadsPath = Path.Combine(webRootPath, "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            uploadsPath = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                return null;
            }
        }

        // Strategy A: If we have both userId and clothingItemId, try direct path
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(clothingItemId))
        {
            var directPath = Path.Combine(uploadsPath, userId, clothingItemId, filename);
            if (File.Exists(directPath))
            {
                return await File.ReadAllBytesAsync(directPath);
            }
            
            // Try with clothingItemId prefix (common pattern: {clothingItemId}_{filename})
            var prefixedFilename = $"{clothingItemId}_{filename}";
            var prefixedPath = Path.Combine(uploadsPath, userId, clothingItemId, prefixedFilename);
            if (File.Exists(prefixedPath))
            {
                return await File.ReadAllBytesAsync(prefixedPath);
            }
        }

        // Strategy B: Search recursively in uploads directory
        try
        {
            var files = Directory.GetFiles(uploadsPath, filename, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                _logger?.LogInformation("Found file {Filename} at {Path}", filename, files[0]);
                return await File.ReadAllBytesAsync(files[0]);
            }

            // Try with wildcards - common patterns include IDs prefixed
            var patternFiles = Directory.GetFiles(uploadsPath, $"*{filename}", SearchOption.AllDirectories);
            if (patternFiles.Length > 0)
            {
                _logger?.LogInformation("Found file matching pattern *{Filename} at {Path}", filename, patternFiles[0]);
                return await File.ReadAllBytesAsync(patternFiles[0]);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error searching for file {Filename} in uploads", filename);
        }

        _logger?.LogWarning("Could not find image file: {Filename}", filename);
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

    /// <summary>
    /// Calculates grid layout based on item count
    /// </summary>
    private GridLayout CalculateGridLayout(int itemCount)
    {
        const int CanvasWidth = 400;
        const int CellHeight = 200;
        
        var layout = new GridLayout { CanvasWidth = CanvasWidth };
        
        switch (itemCount)
        {
            case 1:
                layout.CanvasHeight = 400;
                layout.Cells.Add(new GridCell(0, 0, 400, 400));
                break;
                
            case 2:
                layout.CanvasHeight = 400;
                layout.Cells.Add(new GridCell(0, 0, 200, 400));
                layout.Cells.Add(new GridCell(200, 0, 200, 400));
                break;
                
            case 3:
                layout.CanvasHeight = 400;
                layout.Cells.Add(new GridCell(0, 0, 200, 200));
                layout.Cells.Add(new GridCell(200, 0, 200, 200));
                layout.Cells.Add(new GridCell(0, 200, 400, 200));
                break;
                
            case 4:
                layout.CanvasHeight = 400;
                layout.Cells.Add(new GridCell(0, 0, 200, 200));
                layout.Cells.Add(new GridCell(200, 0, 200, 200));
                layout.Cells.Add(new GridCell(0, 200, 200, 200));
                layout.Cells.Add(new GridCell(200, 200, 200, 200));
                break;
                
            default: // 5+
                int rows = (int)Math.Ceiling(itemCount / 2.0);
                layout.CanvasHeight = rows * CellHeight;
                
                for (int i = 0; i < itemCount; i++)
                {
                    int row = i / 2;
                    int col = i % 2;
                    bool isLastInOddRow = (i == itemCount - 1) && (itemCount % 2 == 1);
                    
                    if (isLastInOddRow)
                    {
                        layout.Cells.Add(new GridCell(0, row * CellHeight, CanvasWidth, CellHeight));
                    }
                    else
                    {
                        layout.Cells.Add(new GridCell(col * 200, row * CellHeight, 200, CellHeight));
                    }
                }
                break;
        }
        
        return layout;
    }

    /// <summary>
    /// Resizes an image to fit within a grid cell while maintaining aspect ratio
    /// </summary>
    private Image<Rgba32> ResizeForCell(Image<Rgba32> image, int cellWidth, int cellHeight)
    {
        // Get content bounds (remove transparent areas)
        var bounds = _processingService.DetectBoundingBox(image);
        using var contentImage = image.Clone(ctx => ctx.Crop(bounds));
        
        // Calculate scaling to fit within cell while maintaining aspect ratio
        double scaleX = (double)cellWidth / contentImage.Width;
        double scaleY = (double)cellHeight / contentImage.Height;
        double scale = Math.Min(scaleX, scaleY) * 0.9; // 90% to leave some padding
        
        int newWidth = (int)(contentImage.Width * scale);
        int newHeight = (int)(contentImage.Height * scale);
        
        // Resize
        return contentImage.Clone(ctx => ctx.Resize(newWidth, newHeight));
    }
}
