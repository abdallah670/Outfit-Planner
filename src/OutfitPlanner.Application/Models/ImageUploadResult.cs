namespace OutfitPlanner.Application.Contracts.Infrastructure.Models;
public record ImageUploadResult
{
    
    public bool Success { get; init; }

    
    public string? OriginalPath { get; init; }

    public string? ThumbnailPath { get; init; }

    public string? MediumPath { get; init; }

 
    public string? LargePath { get; init; }

    public string? ErrorMessage { get; init; }

    public long FileSizeBytes { get; init; }

   
    public int Width { get; init; }

    
    public int Height { get; init; }

 
    public Guid ImageId { get; init; }

  
    public static ImageUploadResult Successful(
        string originalPath,
        string thumbnailPath,
        string mediumPath,
        string largePath,
        long fileSize,
        int width,
        int height,
        Guid imageId) => new()
        {
            Success = true,
            OriginalPath = originalPath,
            ThumbnailPath = thumbnailPath,
            MediumPath = mediumPath,
            LargePath = largePath,
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