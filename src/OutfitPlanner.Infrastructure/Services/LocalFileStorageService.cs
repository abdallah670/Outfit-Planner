using System.IO;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Contracts.Infrastructure.Models;
using OutfitPlanner.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace OutfitPlanner.Infrastructure.Services
{
    public class LocalFileStorageService : IImageStorageService
{
    private readonly IImageProcessingService _imageProcessor;
    private readonly ImageStorageSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    // Size suffixes for file naming
    private const string OriginalSuffix = "";
    private const string ThumbnailSuffix = "_thumb";
    private const string MediumSuffix = "_medium";
    private const string LargeSuffix = "_large";

    public LocalFileStorageService(
        IImageProcessingService imageProcessor,
        ImageStorageSettings settings,
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _imageProcessor = imageProcessor;
        _settings = settings;
        _environment = environment;
        _logger = logger;
    }

       public async Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading image: {FileName} for user: {UserId}", fileName, userId);

        try
        {
            // Validate the image first
            var validation = ValidateImage(imageStream, fileName);
            if (!validation.IsValid)
            {
                return ImageUploadResult.Failed(string.Join(", ", validation.Errors));
            }

            // Process the image - generates all sizes
            using var processedImage = await _imageProcessor.ProcessImageAsync(
                imageStream,
                fileName,
                cancellationToken);

            // Create folder structure: uploads/{userId}/{imageId}/
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                _settings.LocalStoragePath,
                userId,
                processedImage.ImageId.ToString());

            Directory.CreateDirectory(uploadFolder);

            // Save all image variants
            var baseFileName = processedImage.FileName;

            var originalPath = await SaveImageAsync(
                processedImage.Original,
                uploadFolder,
                $"{baseFileName}{OriginalSuffix}.jpg",
                cancellationToken);

            var thumbnailPath = await SaveImageAsync(
                processedImage.Thumbnail,
                uploadFolder,
                $"{baseFileName}{ThumbnailSuffix}.jpg",
                cancellationToken);

            var mediumPath = await SaveImageAsync(
                processedImage.Medium,
                uploadFolder,
                $"{baseFileName}{MediumSuffix}.jpg",
                cancellationToken);

            var largePath = await SaveImageAsync(
                processedImage.Large,
                uploadFolder,
                $"{baseFileName}{LargeSuffix}.jpg",
                cancellationToken);

            // Return relative paths for database storage
            var relativeBase = $"{_settings.LocalStoragePath}/{userId}/{processedImage.ImageId}";

            _logger.LogInformation(
                "Image uploaded successfully: {ImageId} for user: {UserId}",
                processedImage.ImageId, userId);

            return ImageUploadResult.Successful(
                originalPath: $"{relativeBase}/{baseFileName}.jpg",
                thumbnailPath: $"{relativeBase}/{baseFileName}{ThumbnailSuffix}.jpg",
                mediumPath: $"{relativeBase}/{baseFileName}{MediumSuffix}.jpg",
                largePath: $"{relativeBase}/{baseFileName}{LargeSuffix}.jpg",
                fileSize: processedImage.Metadata.SizeBytes,
                width: processedImage.Metadata.Width,
                height: processedImage.Metadata.Height,
                imageId: processedImage.ImageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", fileName);
            return ImageUploadResult.Failed($"Upload failed: {ex.Message}");
        }
    }

      public Task<bool> DeleteImageAsync(
        string imagePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting image: {ImagePath}", imagePath);

        try
        {
            // Extract folder path from image path
            // Path format: uploads/{userId}/{imageId}/{filename}.jpg
            var pathParts = imagePath.Split('/');
            if (pathParts.Length >= 3)
            {
                var folderPath = Path.Combine(
                    _environment.WebRootPath,
                    pathParts[0], // uploads
                    pathParts[1], // userId
                    pathParts[2]); // imageId

                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, recursive: true);
                    _logger.LogInformation("Image folder deleted: {FolderPath}", folderPath);
                    return Task.FromResult(true);
                }
            }

            _logger.LogWarning("Image folder not found: {ImagePath}", imagePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {ImagePath}", imagePath);
            return Task.FromResult(false);
        }
    }

     public string GetImageUrl(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return string.Empty;

        // If base URL is configured, prepend it
        if (!string.IsNullOrEmpty(_settings.BaseUrl))
        {
            return $"{_settings.BaseUrl.TrimEnd('/')}/{imagePath}";
        }

        // Otherwise return relative path
        return $"/{imagePath}";
    }


    public string GetThumbnailUrl(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return string.Empty;

        // Replace the filename to point to thumbnail version
        var directory = Path.GetDirectoryName(imagePath)?.Replace("\\", "/");
        var fileName = Path.GetFileNameWithoutExtension(imagePath);

        return $"{directory}/{fileName}{ThumbnailSuffix}.jpg";
    }

      public Task<bool> ImageExistsAsync(
        string imagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, imagePath);
        return Task.FromResult(File.Exists(fullPath));
    }
         public ImageValidationResult ValidateImage(Stream imageStream, string fileName)
    {
        var errors = new List<string>();

        // Validate file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            errors.Add($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        // Validate file size
        if (imageStream.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024 * 1024);
            errors.Add($"File size exceeds maximum allowed size of {maxSizeMB}MB");
        }

        // Validate image can be loaded
        try
        {
            imageStream.Position = 0;
            using var image = SixLabors.ImageSharp.Image.Load(imageStream);

            // Validate minimum dimensions
            if (image.Width < 50 || image.Height < 50)
            {
                errors.Add("Image dimensions must be at least 50x50 pixels");
            }

            // Validate maximum dimensions
            if (image.Width > 4000 || image.Height > 4000)
            {
                errors.Add("Image dimensions must not exceed 4000x4000 pixels");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Invalid image file: {ex.Message}");
        }

        return errors.Count == 0
            ? ImageValidationResult.Valid()
            : ImageValidationResult.Invalid(errors.ToArray());
    }

    /// <summary>
    /// Saves an image stream to disk.
    /// </summary>
    private async Task<string> SaveImageAsync(
        Stream imageStream,
        string folderPath,
        string fileName,
        CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(folderPath, fileName);
        imageStream.Position = 0;
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await imageStream.CopyToAsync(fileStream, cancellationToken);

        return filePath;
    }
    private async Task DeleteProfileImageAsync(string userId, CancellationToken cancellationToken = default)
    {
        
        if (userId != null)
        {
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "profile_pictures");
            var filePath = Path.Combine(uploadFolder, $"{userId}.png");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    public async Task<ProfileImageUploadResult> UploadProfilePictureAsync(
        Stream imageStream,
        string fileName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading profile picture for user: {UserId}", userId);

        try
        {
            // Validate the image first
            var validation = ValidateImage(imageStream, fileName);
            if (!validation.IsValid)
            {
                return ProfileImageUploadResult.Failed(string.Join(", ", validation.Errors));
            }
            //check if user want to update their profile picture or upload it for first time
            if (userId != null)
            {
                await DeleteProfileImageAsync(userId, cancellationToken);
            }
            // Create folder structure: profile_pictures/
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "profile_pictures");
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            //check if folder exists
            if(!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var originalPath = await SaveImageAsync(
                imageStream,
                uploadFolder,
                $"{userId}{extension}",
                cancellationToken);

            // Return relative paths for database storage
            var relativeBase = $"profile_pictures/{userId}{extension}";

            _logger.LogInformation(
                "Image uploaded successfully: {UserId} for user: {UserId}",
                userId, userId);

            return ProfileImageUploadResult.Successful(
                originalPath: relativeBase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", fileName);
            return ProfileImageUploadResult.Failed($"Upload failed: {ex.Message}");
        }
    }
    }
}
