using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Service that implements AI guide-style image processing for clothing items
/// </summary>
public class OutfitImageProcessingService : IOutfitImageProcessingService
{
    private readonly OutfitLayoutConfig _config;
    private readonly IBackgroundRemovalService? _backgroundRemovalService;
    private readonly LocalBackgroundRemovalService? _localBackgroundRemovalService; // LOCAL - no API calls!
    private const int AlphaThreshold = 10; // Pixels with alpha <= 10 are considered transparent

    public OutfitImageProcessingService(
        IBackgroundRemovalService? backgroundRemovalService = null,
        OutfitLayoutConfig? config = null)
    {
        _backgroundRemovalService = backgroundRemovalService;
        _config = config ?? new OutfitLayoutConfig();
        
        // Use LOCAL background removal for fast performance (no API calls!)
        _localBackgroundRemovalService = new LocalBackgroundRemovalService();
    }

    /// <inheritdoc />
    public Rectangle DetectBoundingBox(Image<Rgba32> image)
    {
        int minX = image.Width;
        int minY = image.Height;
        int maxX = 0;
        int maxY = 0;
        bool foundPixel = false;

        // Scan all pixels to find the bounding box of non-transparent pixels
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];

                // Check if pixel is not transparent (alpha > threshold)
                if (pixel.A > AlphaThreshold)
                {
                    foundPixel = true;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        // If no non-transparent pixels found, return full image bounds
        if (!foundPixel)
        {
            return new Rectangle(0, 0, image.Width, image.Height);
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        return new Rectangle(minX, minY, width, height);
    }

    /// <inheritdoc />
    public Image<Rgba32> CropToBoundingBox(Image<Rgba32> image, Rectangle boundingBox)
    {
        // Ensure bounding box is within image bounds
        boundingBox = Rectangle.Intersect(boundingBox, new Rectangle(0, 0, image.Width, image.Height));
        
        if (boundingBox.Width <= 0 || boundingBox.Height <= 0)
        {
            // Return a clone of the original if invalid bounding box
            return image.Clone();
        }

        // Clone the region defined by the bounding box
        return image.Clone(ctx => ctx.Crop(boundingBox));
    }

    /// <inheritdoc />
    public Image<Rgba32> AutoRotateIfHorizontal(Image<Rgba32> image)
    {
        // If image is significantly wider than tall, rotate it 90 degrees
        if (image.Width > image.Height * 1.2) // 20% threshold to avoid rotating nearly-square images
        {
            return image.Clone(ctx => ctx.Rotate(90));
        }
        
        return image.Clone();
    }

    /// <inheritdoc />
    public Image<Rgba32> ScaleByCategory(Image<Rgba32> image, ClothingType clothingType)
    {
        // Get target width for this clothing type
        if (!_config.CategoryWidths.TryGetValue(clothingType, out int targetWidth))
        {
            targetWidth = 400; // Default width
        }

        // Calculate scale factor while maintaining aspect ratio
        double scaleFactor = (double)targetWidth / image.Width;
        int newHeight = (int)(image.Height * scaleFactor);

        // Resize the image
        return image.Clone(ctx => ctx.Resize(targetWidth, newHeight));
    }

    /// <inheritdoc />
    public Image<Rgba32> CenterOnCanvas(Image<Rgba32> image, int canvasWidth, int canvasHeight)
    {
        // Create a new canvas with white background
        var canvas = new Image<Rgba32>(canvasWidth, canvasHeight);
        
        // Fill with white background
        canvas.Mutate(x => x.BackgroundColor(Color.White));
        
        // Calculate center position
        int centerX = (canvasWidth - image.Width) / 2;
        int centerY = (canvasHeight - image.Height) / 2;
        
        // Ensure non-negative positions
        centerX = Math.Max(0, centerX);
        centerY = Math.Max(0, centerY);
        
        // Draw the image centered on the canvas
        canvas.Mutate(x => x.DrawImage(image, new Point(centerX, centerY), 1f));
        
        return canvas;
    }

    /// <inheritdoc />
    public async Task<Image<Rgba32>> ProcessClothingItemAsync(
        Stream imageStream, 
        ClothingType clothingType,
        CancellationToken cancellationToken = default)
    {
        // Step 0: Use LOCAL background removal (fast, no API calls!)
        Stream processedStream = imageStream;
        if (_localBackgroundRemovalService != null)
        {
            // Use local background removal - much faster than API!
            var bgRemovedBytes = await _localBackgroundRemovalService.RemoveBackgroundAsync(
                imageStream, 
                $"clothing-{clothingType}.jpg",
                cancellationToken);
            processedStream = new MemoryStream(bgRemovedBytes);
        }
        /* DISABLED - Using LOCAL background removal instead of API
        else if (_backgroundRemovalService?.IsConfigured == true)
        {
            var bgRemovedBytes = await _backgroundRemovalService.RemoveBackgroundAsync(
                imageStream, 
                $"clothing-{clothingType}.jpg",
                cancellationToken);
            processedStream = new MemoryStream(bgRemovedBytes);
        }
        */

        // Load the image
        using var originalImage = await Image.LoadAsync<Rgba32>(processedStream, cancellationToken);
        
        // Step 1: Detect bounding box
        var boundingBox = DetectBoundingBox(originalImage);
        
        // Step 2: Crop to bounding box
        using var croppedImage = CropToBoundingBox(originalImage, boundingBox);
        
        // Step 3: Auto-rotate if horizontal
        using var rotatedImage = AutoRotateIfHorizontal(croppedImage);
        
        // Step 4: Scale by category
        using var scaledImage = ScaleByCategory(rotatedImage, clothingType);
        
        // Step 5: Center on standard canvas
        var finalImage = CenterOnCanvas(scaledImage, _config.CanvasWidth, _config.CanvasHeight);
        
        return finalImage;
    }

    /// <summary>
    /// Processes multiple clothing items and returns them with their layout positions
    /// OPTIMIZED: Uses parallel processing and single image load per item
    /// </summary>
    public async Task<List<(ProcessedClothingItem Item, Point Position)>> ProcessOutfitItemsAsync(
        List<(Stream ImageStream, ClothingType Type, string Name)> items,
        CancellationToken cancellationToken = default)
    {
        if (items == null || items.Count == 0)
            return new List<(ProcessedClothingItem, Point)>();

        // Process all items in PARALLEL using Task.WhenAll - much faster than sequential
        var processingTasks = items.Select(async item =>
        {
            // Reset stream position
            item.ImageStream.Position = 0;

            // Step 0: Use LOCAL background removal (fast, no API calls!)
            Stream processedStream = item.ImageStream;
            if (_localBackgroundRemovalService != null)
            {
                // Reset stream
                item.ImageStream.Position = 0;
                
                // Use local background removal - much faster than API!
                var bgRemovedBytes = await _localBackgroundRemovalService.RemoveBackgroundAsync(
                    item.ImageStream,
                    $"clothing-{item.Type}.jpg",
                    cancellationToken);
                processedStream = new MemoryStream(bgRemovedBytes);
            }
            /* DISABLED - Using LOCAL background removal instead of API
            else if (_backgroundRemovalService?.IsConfigured == true)
            {
                item.ImageStream.Position = 0;
                var bgRemovedBytes = await _backgroundRemovalService.RemoveBackgroundAsync(
                    item.ImageStream,
                    $"clothing-{item.Type}.jpg",
                    cancellationToken);
                processedStream = new MemoryStream(bgRemovedBytes);
            }
            */

            // Load the image ONCE (not twice like before)
            using var originalImage = await Image.LoadAsync<Rgba32>(processedStream, cancellationToken);
            
            // Get original dimensions before processing
            var originalWidth = originalImage.Width;
            var originalHeight = originalImage.Height;

            // Step 1: Detect bounding box
            var boundingBox = DetectBoundingBox(originalImage);

            // Step 2: Crop to bounding box
            using var croppedImage = CropToBoundingBox(originalImage, boundingBox);

            // Step 3: Auto-rotate if horizontal
            using var rotatedImage = AutoRotateIfHorizontal(croppedImage);

            // Step 4: Scale by category
            using var scaledImage = ScaleByCategory(rotatedImage, item.Type);

            // Step 5: Center on standard canvas
            var finalImage = CenterOnCanvas(scaledImage, _config.CanvasWidth, _config.CanvasHeight);

            // Get bounding box of final processed image
            var finalBoundingBox = DetectBoundingBox(finalImage);

            return new ProcessedClothingItem
            {
                Image = finalImage, // Don't dispose - we return this!
                Type = item.Type,
                Name = item.Name,
                OriginalWidth = originalWidth,
                OriginalHeight = originalHeight,
                BoundingBox = finalBoundingBox
            };
        });

        // Wait for all items to process in parallel
        var processedItems = (await Task.WhenAll(processingTasks)).ToList();
        
        // Sort items by type order: Tops -> Dresses -> Outerwear -> Bottoms -> Footwear -> Accessories
        var typeOrder = new Dictionary<ClothingType, int>
        {
            { ClothingType.Top, 1 },
            { ClothingType.Dress, 2 },
            { ClothingType.Outerwear, 3 },
            { ClothingType.Bottom, 4 },
            { ClothingType.Activewear, 5 },
            { ClothingType.Swimwear, 6 },
            { ClothingType.Footwear, 7 },
            { ClothingType.Accessory, 8 },
            { ClothingType.Undergarment, 9 }
        };
        
        var sortedItems = processedItems
            .OrderBy(p => typeOrder.GetValueOrDefault(p.Type, 99))
            .ToList();
        
        // Calculate positions for vertical stacking layout
        var result = new List<(ProcessedClothingItem Item, Point Position)>();
        int currentY = _config.Padding;
        int centerX = _config.CanvasWidth / 2;
        
        foreach (var item in sortedItems)
        {
            // Get the actual content bounds from the centered image
            // The item is already centered on its own canvas, so we need to find where the actual content is
            var contentBounds = DetectBoundingBox(item.Image);
            
            // Calculate X position to center the content horizontally
            int x = centerX - (contentBounds.Width / 2);
            
            // For Y, we position based on the actual content, not the full canvas
            int y = currentY - contentBounds.Y; // Adjust for the content offset within the centered canvas
            
            result.Add((item, new Point(x, currentY)));
            
            // Move Y position for next item
            currentY += contentBounds.Height + _config.VerticalSpacing;
        }
        
        return result;
    }
}
