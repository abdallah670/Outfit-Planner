namespace OutfitPlanner.Application.Contracts;

/// <summary>
/// Service for caching and retrieving pre-generated outfit images
/// </summary>
public interface IOutfitImageCacheService
{
    /// <summary>
    /// Gets the cached outfit image if it exists
    /// </summary>
    /// <param name="outfitId">The outfit ID</param>
    /// <returns>Byte array of the cached image, or null if not found</returns>
    Task<byte[]?> GetCachedImageAsync(Guid outfitId);
    
    /// <summary>
    /// Caches an outfit image for future retrieval
    /// </summary>
    /// <param name="outfitId">The outfit ID</param>
    /// <param name="imageData">The image byte array</param>
    /// <returns>Path to the cached image file</returns>
    Task<string> CacheImageAsync(Guid outfitId, byte[] imageData);
    
    /// <summary>
    /// Checks if a cached image exists for the outfit
    /// </summary>
    /// <param name="outfitId">The outfit ID</param>
    /// <returns>True if cached image exists</returns>
    Task<bool> IsImageCachedAsync(Guid outfitId);
    
    /// <summary>
    /// Deletes the cached image for an outfit
    /// </summary>
    /// <param name="outfitId">The outfit ID</param>
    Task DeleteCachedImageAsync(Guid outfitId);
    
    /// <summary>
    /// Gets the URL path for the cached image
    /// </summary>
    /// <param name="outfitId">The outfit ID</param>
    /// <returns>Relative URL path to the image</returns>
    string GetCachedImageUrl(Guid outfitId);
}

/// <summary>
/// Configuration for outfit image caching
/// </summary>
public class OutfitImageCacheSettings
{
    public const string SectionName = "OutfitImageCache";
    
    /// <summary>
    /// Enable pre-generation of outfit images on creation
    /// </summary>
    public bool EnablePreGeneration { get; set; } = true;
    
    /// <summary>
    /// Directory to store cached images (relative to web root)
    /// </summary>
    public string CacheDirectory { get; set; } = "uploads/outfit-images";
    
    /// <summary>
    /// Maximum age of cached images in days before regeneration
    /// </summary>
    public int MaxCacheAgeDays { get; set; } = 30;
    
    /// <summary>
    /// Image quality (1-100) for JPEG output
    /// </summary>
    public int ImageQuality { get; set; } = 90;
}
