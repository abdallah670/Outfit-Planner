namespace OutfitPlanner.Application.Contracts.Infrastructure.Models;
public record ProfileImageUploadResult
{
    
    public bool Success { get; init; }

    
    public string? OriginalPath { get; init; }


    public string? ErrorMessage { get; init; }


    
    
    public static ProfileImageUploadResult Successful(
        string originalPath) => new()
        {
            Success = true,
            OriginalPath = originalPath,
        };

  
    
    public static ProfileImageUploadResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
