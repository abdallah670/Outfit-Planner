using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Infrastructure.Configuration;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Service for caching pre-generated outfit images to disk
/// </summary>
public class OutfitImageCacheService : IOutfitImageCacheService
{
    private readonly OutfitImageCacheSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<OutfitImageCacheService> _logger;

    public OutfitImageCacheService(
        IOptions<OutfitImageCacheSettings> settings,
        IWebHostEnvironment environment,
        ILogger<OutfitImageCacheService> logger)
    {
        _settings = settings.Value;
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<byte[]?> GetCachedImageAsync(Guid outfitId)
    {
        var filePath = GetCacheFilePath(outfitId);
        
        if (!File.Exists(filePath))
        {
            return Task.FromResult<byte[]?>(null);
        }

        // Check if cache is expired
        var fileInfo = new FileInfo(filePath);
        if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > TimeSpan.FromDays(_settings.MaxCacheAgeDays))
        {
            _logger.LogInformation("Cached image for outfit {OutfitId} has expired", outfitId);
            return Task.FromResult<byte[]?>(null);
        }

        var imageData = File.ReadAllBytes(filePath);
        return Task.FromResult<byte[]?>(imageData);
    }

    /// <inheritdoc />
    public async Task<string> CacheImageAsync(Guid outfitId, byte[] imageData)
    {
        var cacheDir = GetCacheDirectory();
        
        // Ensure cache directory exists
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
            _logger.LogInformation("Created outfit image cache directory: {CacheDir}", cacheDir);
        }

        var filePath = GetCacheFilePath(outfitId);
        await File.WriteAllBytesAsync(filePath, imageData);
        
        _logger.LogInformation("Cached outfit image for outfit {OutfitId} at {FilePath}", outfitId, filePath);
        
        return GetCachedImageUrl(outfitId);
    }

    /// <inheritdoc />
    public Task<bool> IsImageCachedAsync(Guid outfitId)
    {
        var filePath = GetCacheFilePath(outfitId);
        var exists = File.Exists(filePath);
        
        if (exists)
        {
            // Check if cache is expired
            var fileInfo = new FileInfo(filePath);
            if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > TimeSpan.FromDays(_settings.MaxCacheAgeDays))
            {
                return Task.FromResult(false);
            }
        }
        
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public Task DeleteCachedImageAsync(Guid outfitId)
    {
        var filePath = GetCacheFilePath(outfitId);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted cached image for outfit {OutfitId}", outfitId);
        }
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GetCachedImageUrl(Guid outfitId)
    {
        return $"/uploads/outfit-images/outfit-{outfitId}.jpg";
    }

    private string GetCacheDirectory()
    {
        return Path.Combine(_environment.WebRootPath, _settings.CacheDirectory);
    }

    private string GetCacheFilePath(Guid outfitId)
    {
        var cacheDir = GetCacheDirectory();
        return Path.Combine(cacheDir, $"outfit-{outfitId}.jpg");
    }
}
