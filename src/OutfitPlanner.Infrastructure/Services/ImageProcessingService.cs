using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Contracts.Infrastructure.Models;
using OutfitPlanner.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using ImageMetadata = OutfitPlanner.Application.Contracts.Infrastructure.Models.ImageMetadata;

namespace OutfitPlanner.Infrastructure.Services;
public class ImageProcessingService : IImageProcessingService
{
    private readonly ImageStorageSettings _settings;
    private readonly ILogger<ImageProcessingService> _logger;
    public ImageProcessingService(
        ImageStorageSettings settings,
        ILogger<ImageProcessingService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<ProcessedImage> ProcessImageAsync(
        Stream imageStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing image: {FileName}", fileName);

        try
        {
            // Reset stream position before reading since it might have been read by validation
            imageStream.Position = 0;
            
            // Load the original image
            using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);

            var imageId = Guid.NewGuid();
            var extension = Path.GetExtension(fileName);
            // Fix: Extract just the filename without directory path
            var baseFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
            var generatedFileName = $"{imageId}_{baseFileName}";

            var result = new ProcessedImage
            {
                FileName = generatedFileName,
                Extension = ".jpg", // Always convert to JPEG for consistency
                ImageId = imageId,
                Metadata = ExtractMetadata(originalImage, imageStream.Length)
            };

            // Generate thumbnail
            result.Thumbnail = await ResizeToStreamAsync(
                originalImage,
                _settings.Thumbnails.ThumbnailSize,
                _settings.Thumbnails.ThumbnailSize,
                _settings.Thumbnails.ThumbnailQuality,
                cancellationToken);

            // Generate medium
            result.Medium = await ResizeToStreamAsync(
                originalImage,
                _settings.Thumbnails.MediumSize,
                _settings.Thumbnails.MediumSize,
                _settings.Thumbnails.MediumQuality,
                cancellationToken);

            // Generate large
            result.Large = await ResizeToStreamAsync(
                originalImage,
                _settings.Thumbnails.LargeSize,
                _settings.Thumbnails.LargeSize,
                _settings.Thumbnails.LargeQuality,
                cancellationToken);

            // Store original - convert to JPEG for consistency
            result.Original = await ConvertToJpegAsync(
                imageStream,
                _settings.Thumbnails.LargeQuality,
                cancellationToken);

            _logger.LogInformation(
                "Image processed successfully: {FileName}, Original size: {Width}x{Height}",
                fileName, result.Metadata.Width, result.Metadata.Height);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image: {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Stream> ResizeImageAsync(
        Stream imageStream,
        int maxWidth,
        int maxHeight,
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        return await ResizeToStreamAsync(image, maxWidth, maxHeight, quality, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ImageMetadata> GetMetadataAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        return ExtractMetadata(image, imageStream.Length);
    }

    /// <inheritdoc />
    public async Task<Stream> ConvertToJpegAsync(
        Stream imageStream,
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        var outputStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = quality };
        await image.SaveAsync(outputStream, encoder, cancellationToken);
        outputStream.Position = 0;

        return outputStream;
    }

    /// <summary>
    /// Resizes an image to the specified dimensions and returns as a stream.
    /// </summary>
    private async Task<Stream> ResizeToStreamAsync(
        Image image,
        int maxWidth,
        int maxHeight,
        int quality,
        CancellationToken cancellationToken)
    {
        var outputStream = new MemoryStream();

        // Clone the image for resizing
        using var clone = image.Clone(context =>
        {
            context.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = _settings.Thumbnails.MaintainAspectRatio
                    ? ResizeMode.Max  // Maintains aspect ratio, fits within bounds
                    : ResizeMode.Stretch, // Stretches to exact size
                Sampler = KnownResamplers.Lanczos3 // High quality resampling
            });
        });

        var encoder = new JpegEncoder { Quality = quality };
        await clone.SaveAsync(outputStream, encoder, cancellationToken);
        outputStream.Position = 0;

        return outputStream;
    }

    /// <summary>
    /// Extracts metadata from an image.
    /// </summary>
    private ImageMetadata ExtractMetadata(Image image, long streamLength)
    {
        var metadata = image.Metadata;

        return new ImageMetadata
        {
            Width = image.Width,
            Height = image.Height,
            Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
            SizeBytes = streamLength,
            BitsPerPixel = image.PixelType.BitsPerPixel,
            HasTransparency = image.PixelType.AlphaRepresentation != PixelAlphaRepresentation.None,
            DpiX = metadata.HorizontalResolution,
            DpiY = metadata.VerticalResolution
        };
    }
}