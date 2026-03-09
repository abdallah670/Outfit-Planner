using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Contracts;

/// <summary>
/// Service for processing individual clothing item images for outfit composition
/// </summary>
public interface IOutfitImageProcessingService
{
    /// <summary>
    /// Detects the bounding box of the actual clothing object (ignoring transparent areas)
    /// </summary>
    Rectangle DetectBoundingBox(Image<Rgba32> image);
    
    /// <summary>
    /// Crops image to the detected bounding box
    /// </summary>
    Image<Rgba32> CropToBoundingBox(Image<Rgba32> image, Rectangle boundingBox);
    
    /// <summary>
    /// Automatically rotates image if it's horizontal (width > height)
    /// </summary>
    Image<Rgba32> AutoRotateIfHorizontal(Image<Rgba32> image);
    
    /// <summary>
    /// Scales image based on clothing category/type
    /// </summary>
    Image<Rgba32> ScaleByCategory(Image<Rgba32> image, ClothingType clothingType);
    
    /// <summary>
    /// Centers image on a canvas of specified size
    /// </summary>
    Image<Rgba32> CenterOnCanvas(Image<Rgba32> image, int canvasWidth, int canvasHeight);
    
    /// <summary>
    /// Complete processing pipeline for a single clothing item
    /// </summary>
    Task<Image<Rgba32>> ProcessClothingItemAsync(
        Stream imageStream, 
        ClothingType clothingType,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes multiple clothing items and returns them with their layout positions
    /// </summary>
    Task<List<(ProcessedClothingItem Item, Point Position)>> ProcessOutfitItemsAsync(
        List<(Stream ImageStream, ClothingType Type, string Name)> items,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Layout configuration for outfit composition
/// </summary>
public class OutfitLayoutConfig
{
    public int CanvasWidth { get; set; } = 1024;
    public int CanvasHeight { get; set; } = 1024;
    public int Padding { get; set; } = 50;
    
    /// <summary>
    /// Target widths for different clothing types (in pixels)
    /// </summary>
    public Dictionary<ClothingType, int> CategoryWidths { get; set; } = new()
    {
        { ClothingType.Top, 500 },
        { ClothingType.Bottom, 450 },
        { ClothingType.Dress, 500 },
        { ClothingType.Outerwear, 550 },
        { ClothingType.Footwear, 350 },
        { ClothingType.Accessory, 250 },
        { ClothingType.Undergarment, 400 },
        { ClothingType.Swimwear, 450 },
        { ClothingType.Activewear, 480 }
    };
    
    /// <summary>
    /// Vertical spacing between items (in pixels)
    /// </summary>
    public int VerticalSpacing { get; set; } = 30;
}

/// <summary>
/// Represents a processed clothing item ready for composition
/// </summary>
public class ProcessedClothingItem
{
    public required Image<Rgba32> Image { get; set; }
    public required ClothingType Type { get; set; }
    public required string Name { get; set; }
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public Rectangle BoundingBox { get; set; }
}
