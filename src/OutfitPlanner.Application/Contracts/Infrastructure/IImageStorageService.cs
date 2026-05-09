using OutfitPlanner.Application.Contracts.Infrastructure.Models;

namespace OutfitPlanner.Application.Contracts.Infrastructure{
    public interface IImageStorageService
  {
     Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string userId,
        CancellationToken cancellationToken = default);
    Task <ProfileImageUploadResult> UploadProfilePictureAsync(
        Stream imageStream,
        string fileName,
        string userId,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(
        string imagePath,
        CancellationToken cancellationToken = default);
     string GetImageUrl(string imagePath);
     string GetThumbnailUrl(string imagePath);
         Task<bool> ImageExistsAsync(
        string imagePath,
        CancellationToken cancellationToken = default);
    ImageValidationResult ValidateImage(Stream imageStream, string fileName);
     




    
  }
}
