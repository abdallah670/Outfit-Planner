namespace OutfitPlanner.Application.Contracts.Infrastructure.Models{
public record ImageMetadata
{
    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Image format - JPEG, PNG, WebP, etc.
    /// </summary>
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Bits per pixel.
    /// </summary>
    public int BitsPerPixel { get; init; }

    /// <summary>
    /// Whether the image has transparency.
    /// </summary>
    public bool HasTransparency { get; init; }

    /// <summary>
    /// DPI horizontal resolution.
    /// </summary>
    public double DpiX { get; init; }

    /// <summary>
    /// DPI vertical resolution.
    /// </summary>
    public double DpiY { get; init; }
}
}
