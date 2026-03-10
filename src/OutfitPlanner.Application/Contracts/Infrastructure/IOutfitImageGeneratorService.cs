using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Contracts;

/// <summary>
/// Service for generating outfit images by combining clothing item images
/// </summary>
public interface IOutfitImageGeneratorService
{
    /// <summary>
    /// Generates and caches an outfit image from its clothing items
    /// </summary>
    /// <param name="outfit">The outfit entity with loaded clothing items</param>
    /// <returns>The URL path to the generated image, or null if generation failed</returns>
    Task<string?> GenerateOutfitImageAsync(Outfit outfit);
    
    /// <summary>
    /// Regenerates the outfit image (useful after outfit updates)
    /// </summary>
    /// <param name="outfit">The outfit entity with loaded clothing items</param>
    /// <returns>The URL path to the regenerated image, or null if generation failed</returns>
    Task<string?> RegenerateOutfitImageAsync(Outfit outfit);
    
    /// <summary>
    /// Deletes the cached outfit image
    /// </summary>
    /// <param name="outfitId">The outfit ID</param>
    Task DeleteOutfitImageAsync(Guid outfitId);
}
