namespace OutfitPlanner.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for image storage.
/// </summary>
public class ImageStorageSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "ImageStorage";

    /// <summary>
    /// Storage provider to use.
    /// </summary>
    public StorageProvider Provider { get; set; } = StorageProvider.LocalFileSystem;

    /// <summary>
    /// Local file system storage path - relative to wwwroot.
    /// </summary>
    public string LocalStoragePath { get; set; } = "uploads";

    /// <summary>
    /// Azure Blob Storage connection string.
    /// Required when Provider is AzureBlobStorage.
    /// </summary>
    public string? AzureConnectionString { get; set; }

    /// <summary>
    /// Azure Blob Storage container name.
    /// </summary>
    public string AzureContainerName { get; set; } = "clothing-images";

    /// <summary>
    /// Maximum file size in bytes. Default: 10MB.
    /// </summary>
    public int MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Allowed file extensions.
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new() { ".jpg", ".jpeg", ".png", ".webp" };

    /// <summary>
    /// Allowed MIME types.
    /// </summary>
    public List<string> AllowedMimeTypes { get; set; } = new()
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    /// <summary>
    /// Thumbnail generation settings.
    /// </summary>
    public ThumbnailSettings Thumbnails { get; set; } = new();

    /// <summary>
    /// Base URL for serving images - used for generating full URLs.
    /// </summary>
    public string? BaseUrl { get; set; }
}

/// <summary>
/// Storage provider options.
/// </summary>
public enum StorageProvider
{
    /// <summary>
    /// Store images on local file system.
    /// </summary>
    LocalFileSystem,

    /// <summary>
    /// Store images in Azure Blob Storage.
    /// </summary>
    AzureBlobStorage
}

/// <summary>
/// Thumbnail size and quality settings.
/// </summary>
public class ThumbnailSettings
{
    /// <summary>
    /// Thumbnail size - used for grid views and cards.
    /// </summary>
    public int ThumbnailSize { get; set; } = 150;

    /// <summary>
    /// Medium size - used for detail previews.
    /// </summary>
    public int MediumSize { get; set; } = 400;

    /// <summary>
    /// Large size - used for full detail views.
    /// </summary>
    public int LargeSize { get; set; } = 800;

    /// <summary>
    /// JPEG quality for thumbnails - 1 to 100.
    /// </summary>
    public int ThumbnailQuality { get; set; } = 75;

    /// <summary>
    /// JPEG quality for medium images - 1 to 100.
    /// </summary>
    public int MediumQuality { get; set; } = 85;

    /// <summary>
    /// JPEG quality for large images - 1 to 100.
    /// </summary>
    public int LargeQuality { get; set; } = 90;

    /// <summary>
    /// Whether to maintain aspect ratio.
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>
    /// Background color for padding if aspect ratio is maintained.
    /// </summary>
    public string PaddingColor { get; set; } = "#FFFFFF";
}