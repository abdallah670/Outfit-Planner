using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts;
using OutfitPlanner.Infrastructure.Configuration;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Background removal service using remove.bg API
/// </summary>
public class RemoveBgBackgroundRemovalService : IBackgroundRemovalService
{
    private readonly HttpClient _httpClient;
    private readonly BackgroundRemovalSettings _settings;
    private readonly ILogger<RemoveBgBackgroundRemovalService> _logger;
    private const string RemoveBgApiUrl = "https://api.remove.bg/v1.0/removebg";

    public RemoveBgBackgroundRemovalService(
        IOptions<BackgroundRemovalSettings> settings,
        ILogger<RemoveBgBackgroundRemovalService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds)
        };
    }

    /// <inheritdoc />
    public bool IsConfigured => 
        _settings.Enabled && 
        !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
        _settings.Provider.Equals("RemoveBg", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<byte[]> RemoveBackgroundAsync(
        Stream imageStream, 
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Background removal service is not configured. Returning original image.");
            return await ReadStreamToByteArrayAsync(imageStream);
        }

        try
        {
            _logger.LogInformation("Removing background from image: {FileName}", fileName);
            
            // Reset stream position
            imageStream.Position = 0;
            
            // Check image size
            if (imageStream.Length > _settings.MaxImageSizeMb * 1024 * 1024)
            {
                _logger.LogWarning("Image {FileName} exceeds maximum size of {MaxSize}MB. Returning original.", 
                    fileName, _settings.MaxImageSizeMb);
                return await ReadStreamToByteArrayAsync(imageStream);
            }

            // Create multipart form content
            var formContent = new MultipartFormDataContent();
            
            // Add image file
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            formContent.Add(streamContent, "image_file", fileName);
            
            // Add format parameter (PNG for transparency)
            formContent.Add(new StringContent("png"), "format");
            
            // Add API key header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _settings.ApiKey);

            // Call remove.bg API
            var response = await _httpClient.PostAsync(RemoveBgApiUrl, formContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                _logger.LogInformation("Successfully removed background from {FileName}", fileName);
                return result;
            }
            
            // Log error details
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Remove.bg API error: {StatusCode} - {Error}", 
                response.StatusCode, errorContent);
            
            // Return original image on API error
            imageStream.Position = 0;
            return await ReadStreamToByteArrayAsync(imageStream);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Background removal request timed out for {FileName}", fileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing background from {FileName}", fileName);
            
            // Return original image on any error
            imageStream.Position = 0;
            return await ReadStreamToByteArrayAsync(imageStream);
        }
    }

    private async Task<byte[]> ReadStreamToByteArrayAsync(Stream stream)
    {
        stream.Position = 0;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
