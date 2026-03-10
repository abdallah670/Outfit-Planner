using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Contracts;

/// <summary>
/// Service for combining multiple clothing item images into a single outfit image
/// </summary>
public interface IImageCombinationService
{
    /// <summary>
    /// Combines multiple images from URLs into a single outfit image with smart layout
    /// </summary>
    Task<byte[]?> CombineImagesAsync(
        IEnumerable<string> imageUrls, 
        IEnumerable<ClothingType> clothingTypes,
        IEnumerable<string> clothingNames,
        string webRootPath, 
        string baseUrl);
    
    /// <summary>
    /// Combines multiple images from local file paths into a single outfit image
    /// </summary>
    Task<byte[]?> CombineImagesFromPathsAsync(
        IEnumerable<string> imagePaths,
        IEnumerable<ClothingType> clothingTypes,
        IEnumerable<string> clothingNames);
    
    /// <summary>
    /// Legacy method for backward compatibility - simple vertical stacking
    /// </summary>
    [Obsolete("Use CombineImagesAsync with clothing types for better results")]
    Task<byte[]?> CombineImagesAsync(IEnumerable<string> imageUrls, string webRootPath, string baseUrl);
    
    /// <summary>
    /// Legacy method for backward compatibility - simple vertical stacking
    /// </summary>
    [Obsolete("Use CombineImagesFromPathsAsync with clothing types for better results")]
    Task<byte[]?> CombineImagesFromPathsAsync(IEnumerable<string> imagePaths);
}

/// <summary>
/// Input data for combining outfit images
/// </summary>
public class OutfitImageInput
{
    public required string ImageUrl { get; set; }
    public required ClothingType ClothingType { get; set; }
    public required string ClothingName { get; set; }
}
