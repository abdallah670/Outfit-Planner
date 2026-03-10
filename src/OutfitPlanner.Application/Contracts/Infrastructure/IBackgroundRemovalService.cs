namespace OutfitPlanner.Application.Contracts;

/// <summary>
/// Service for removing backgrounds from clothing item images
/// </summary>
public interface IBackgroundRemovalService
{
    /// <summary>
    /// Removes background from an image and returns transparent PNG
    /// </summary>
    /// <param name="imageStream">Input image stream</param>
    /// <param name="fileName">Original file name for logging</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Byte array of transparent PNG image</returns>
    Task<byte[]> RemoveBackgroundAsync(
        Stream imageStream, 
        string fileName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the background removal service is configured and available
    /// </summary>
    bool IsConfigured { get; }
}

/// <summary>
/// Configuration for background removal API
/// </summary>
public class BackgroundRemovalSettings
{
    public const string SectionName = "BackgroundRemoval";
    
    /// <summary>
    /// API Provider: "RemoveBg" or "ClipDrop"
    /// </summary>
    public string Provider { get; set; } = "RemoveBg";
    
    /// <summary>
    /// API Key for the selected provider
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Enable background removal (set to false to skip API calls)
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Timeout in seconds for API calls
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum image size in MB
    /// </summary>
    public int MaxImageSizeMb { get; set; } = 10;
}
