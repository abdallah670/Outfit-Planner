// using Azure.Storage.Blobs;
// using Azure.Storage.Blobs.Models;
// using OutfitPlanner.Application.Contracts.Infrastructure;
// using OutfitPlanner.Application.Contracts.Infrastructure.Models;
// using OutfitPlanner.Infrastructure.Configuration;
// using Microsoft.Extensions.Logging;

// namespace OutfitPlanner.Infrastructure.Services;

// /// <summary>
// /// Azure Blob Storage implementation of image storage.
// /// Stores images in Azure Blob Storage with container per environment.
// /// </summary>
// public class AzureBlobStorageService : IImageStorageService
// {
//     private readonly BlobServiceClient _blobServiceClient;
//     private readonly IImageProcessingService _imageProcessor;
//     private readonly ImageStorageSettings _settings;
//     private readonly ILogger<AzureBlobStorageService> _logger;

//     private BlobContainerClient? _containerClient;

//     private const string OriginalSuffix = "";
//     private const string ThumbnailSuffix = "_thumb";
//     private const string MediumSuffix = "_medium";
//     private const string LargeSuffix = "_large";

//     public AzureBlobStorageService(
//         BlobServiceClient blobServiceClient,
//         IImageProcessingService imageProcessor,
//         ImageStorageSettings settings,
//         ILogger<AzureBlobStorageService> logger)
//     {
//         _blobServiceClient = blobServiceClient;
//         _imageProcessor = imageProcessor;
//         _settings = settings;
//         _logger = logger;
//     }

//     /// <inheritdoc />
//     public async Task<ImageUploadResult> UploadImageAsync(
//         Stream imageStream,
//         string fileName,
//         string userId,
//         CancellationToken cancellationToken = default)
//     {
//         _logger.LogInformation("Uploading image to Azure Blob: {FileName} for user: {UserId}", fileName, userId);

//         try
//         {
//             // Validate the image first
//             var validation = ValidateImage(imageStream, fileName);
//             if (!validation.IsValid)
//             {
//                 return ImageUploadResult.Failed(string.Join(", ", validation.Errors));
//             }

//             // Ensure container exists
//             var containerClient = await GetContainerClientAsync(cancellationToken);

//             // Process the image
//             using var processedImage = await _imageProcessor.ProcessImageAsync(
//                 imageStream,
//                 fileName,
//                 cancellationToken);

//             // Blob path structure: {userId}/{imageId}/{filename}.jpg
//             var baseFileName = processedImage.FileName;
//             var blobFolder = $"{userId}/{processedImage.ImageId}";

//             // Upload all variants
//             var originalPath = await UploadBlobAsync(
//                 containerClient,
//                 processedImage.Original,
//                 $"{blobFolder}/{baseFileName}{OriginalSuffix}.jpg",
//                 cancellationToken);

//             var thumbnailPath = await UploadBlobAsync(
//                 containerClient,
//                 processedImage.Thumbnail,
//                 $"{blobFolder}/{baseFileName}{ThumbnailSuffix}.jpg",
//                 cancellationToken);

//             var mediumPath = await UploadBlobAsync(
//                 containerClient,
//                 processedImage.Medium,
//                 $"{blobFolder}/{baseFileName}{MediumSuffix}.jpg",
//                 cancellationToken);

//             var largePath = await UploadBlobAsync(
//                 containerClient,
//                 processedImage.Large,
//                 $"{blobFolder}/{baseFileName}{LargeSuffix}.jpg",
//                 cancellationToken);

//             _logger.LogInformation(
//                 "Image uploaded to Azure Blob successfully: {ImageId} for user: {UserId}",
//                 processedImage.ImageId, userId);

//             return ImageUploadResult.Successful(
//                 originalPath: originalPath,
//                 thumbnailPath: thumbnailPath,
//                 mediumPath: mediumPath,
//                 largePath: largePath,
//                 fileSize: processedImage.Metadata.SizeBytes,
//                 width: processedImage.Metadata.Width,
//                 height: processedImage.Metadata.Height,
//                 imageId: processedImage.ImageId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to upload image to Azure Blob: {FileName}", fileName);
//             return ImageUploadResult.Failed($"Upload failed: {ex.Message}");
//         }
//     }

//     /// <inheritdoc />
//     public async Task<bool> DeleteImageAsync(
//         string imagePath,
//         CancellationToken cancellationToken = default)
//     {
//         _logger.LogInformation("Deleting image from Azure Blob: {ImagePath}", imagePath);

//         try
//         {
//             var containerClient = await GetContainerClientAsync(cancellationToken);

//             // Delete all blobs in the folder
//             var pathParts = imagePath.Split('/');
//             if (pathParts.Length >= 2)
//             {
//                 var folderPrefix = $"{pathParts[0]}/{pathParts[1]}/";

//                 await foreach (var blob in containerClient.GetBlobsAsync(
//                     prefix: folderPrefix,
//                     cancellationToken: cancellationToken))
//                 {
//                     await containerClient.DeleteBlobAsync(blob.Name, cancellationToken: cancellationToken);
//                 }

//                 _logger.LogInformation("Image blobs deleted: {FolderPath}", folderPrefix);
//                 return true;
//             }

//             return false;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to delete image from Azure Blob: {ImagePath}", imagePath);
//             return false;
//         }
//     }

//     /// <inheritdoc />
//     public string GetImageUrl(string imagePath)
//     {
//         if (string.IsNullOrEmpty(imagePath))
//             return string.Empty;

//         // If base URL is configured, use it
//         if (!string.IsNullOrEmpty(_settings.BaseUrl))
//         {
//             return $"{_settings.BaseUrl.TrimEnd('/')}/{imagePath}";
//         }

//         // Otherwise construct from blob service
//         var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.AzureContainerName);
//         return containerClient.Uri + "/" + imagePath;
//     }

//     /// <inheritdoc />
//     public string GetThumbnailUrl(string imagePath)
//     {
//         if (string.IsNullOrEmpty(imagePath))
//             return string.Empty;

//         var directory = Path.GetDirectoryName(imagePath)?.Replace("\\", "/");
//         var fileName = Path.GetFileNameWithoutExtension(imagePath);

//         return $"{directory}/{fileName}{ThumbnailSuffix}.jpg";
//     }

//     /// <inheritdoc />
//     public async Task<bool> ImageExistsAsync(
//         string imagePath,
//         CancellationToken cancellationToken = default)
//     {
//         var containerClient = await GetContainerClientAsync(cancellationToken);
//         var blobClient = containerClient.GetBlobClient(imagePath);
//         return await blobClient.ExistsAsync(cancellationToken);
//     }

//     /// <inheritdoc />
//     public ImageValidationResult ValidateImage(Stream imageStream, string fileName)
//     {
//         var errors = new List<string>();

//         // Validate file extension
//         var extension = Path.GetExtension(fileName).ToLowerInvariant();
//         if (!_settings.AllowedExtensions.Contains(extension))
//         {
//             errors.Add($"File extension '{extension}' is not allowed.");
//         }

//         // Validate file size
//         if (imageStream.Length > _settings.MaxFileSizeBytes)
//         {
//             var maxSizeMB = _settings.MaxFileSizeBytes / (1024 * 1024);
//             errors.Add($"File size exceeds maximum allowed size of {maxSizeMB}MB");
//         }

//         // Validate image can be loaded
//         try
//         {
//             imageStream.Position = 0;
//             using var image = SixLabors.ImageSharp.Image.Load(imageStream);

//             if (image.Width < 50 || image.Height < 50)
//             {
//                 errors.Add("Image dimensions must be at least 50x50 pixels");
//             }
//         }
//         catch (Exception ex)
//         {
//             errors.Add($"Invalid image file: {ex.Message}");
//         }

//         return errors.Count == 0
//             ? ImageValidationResult.Valid()
//             : ImageValidationResult.Invalid(errors.ToArray());
//     }

//     /// <summary>
//     /// Gets or creates the blob container client.
//     /// </summary>
//     private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
//     {
//         if (_containerClient != null)
//             return _containerClient;

//         _containerClient = _blobServiceClient.GetBlobContainerClient(_settings.AzureContainerName);
//         await _containerClient.CreateIfNotExistsAsync(
//             PublicAccessType.Blob,
//             cancellationToken: cancellationToken);

//         return _containerClient;
//     }

//     /// <summary>
//     /// Uploads a blob to Azure Storage.
//     /// </summary>
//     private async Task<string> UploadBlobAsync(
//         BlobContainerClient containerClient,
//         Stream imageStream,
//         string blobName,
//         CancellationToken cancellationToken)
//     {
//         imageStream.Position = 0;

//         var blobClient = containerClient.GetBlobClient(blobName);

//         await blobClient.UploadAsync(
//             imageStream,
//             new BlobUploadOptions
//             {
//                 HttpHeaders = new BlobHttpHeaders
//                 {
//                     ContentType = "image/jpeg"
//                 }
//             },
//             cancellationToken);

//         return blobName;
//     }
// }


