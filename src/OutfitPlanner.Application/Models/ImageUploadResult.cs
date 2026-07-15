namespace OutfitPlanner.Application.Contracts.Infrastructure.Models;
public record ImageUploadResult
{
    public bool Success { get; init; }

    public string? OriginalPath { get; init; }

    public string? ErrorMessage { get; init; }

    public long FileSizeBytes { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public Guid ImageId { get; init; }

    public static ImageUploadResult Successful(
        string originalPath,
        long fileSize,
        int width,
        int height,
        Guid imageId) => new()
        {
            Success = true,
            OriginalPath = originalPath,
            FileSizeBytes = fileSize,
            Width = width,
            Height = height,
            ImageId = imageId
        };

    public static ImageUploadResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
