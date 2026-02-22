using OutfitPlanner.Application.Contracts.Infrastructure.Models;

namespace OutfitPlanner.Application.Contracts.Infrastructure{
    public interface IImageProcessingService{
          Task<ProcessedImage> ProcessImageAsync(
        Stream imageStream,
        string fileName,
        CancellationToken cancellationToken = default);
           Task<Stream> ResizeImageAsync(
        Stream imageStream,
        int maxWidth,
        int maxHeight,
        int quality = 85,
        CancellationToken cancellationToken = default);
          Task<ImageMetadata> GetMetadataAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default);
           Task<Stream> ConvertToJpegAsync(
        Stream imageStream,
        int quality = 85,
        CancellationToken cancellationToken = default);
    }
}