using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OutfitPlanner.Application.Contracts;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Local background removal service using ImageSharp algorithms
/// This removes background without needing an external API - much faster!
/// </summary>
public class LocalBackgroundRemovalService : IBackgroundRemovalService
{
    private const int SimilarityThreshold = 30; // Color distance threshold
    private const int MinBackgroundPixels = 10; // Minimum corner pixels to sample
    
    public bool IsConfigured => true; // Always available
    
    /// <inheritdoc />
    public async Task<byte[]> RemoveBackgroundAsync(
        Stream imageStream, 
        string fileName,
        CancellationToken cancellationToken = default)
    {
        // Load image
        using var image = await Image.LoadAsync<Rgba32>(imageStream, cancellationToken);
        
        // Detect background color from corners
        var backgroundColor = DetectBackgroundColor(image);
        
        // Remove similar background pixels
        RemoveBackgroundPixels(image, backgroundColor);
        
        // Save to PNG with transparency
        using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms, cancellationToken);
        return ms.ToArray();
    }
    
    /// <summary>
    /// Detects the background color by sampling corner pixels
    /// </summary>
    private Rgba32 DetectBackgroundColor(Image<Rgba32> image)
    {
        // Sample colors from all 4 corners
        var cornerColors = new List<Rgba32>();
        
        // Top-left corner
        for (int y = 0; y < Math.Min(MinBackgroundPixels, image.Height); y++)
        {
            for (int x = 0; x < Math.Min(MinBackgroundPixels, image.Width); x++)
            {
                cornerColors.Add(image[x, y]);
            }
        }
        
        // Top-right corner
        for (int y = 0; y < Math.Min(MinBackgroundPixels, image.Height); y++)
        {
            for (int x = image.Width - Math.Min(MinBackgroundPixels, image.Width); x < image.Width; x++)
            {
                cornerColors.Add(image[x, y]);
            }
        }
        
        // Bottom-left corner
        for (int y = image.Height - Math.Min(MinBackgroundPixels, image.Height); y < image.Height; y++)
        {
            for (int x = 0; x < Math.Min(MinBackgroundPixels, image.Width); x++)
            {
                cornerColors.Add(image[x, y]);
            }
        }
        
        // Bottom-right corner
        for (int y = image.Height - Math.Min(MinBackgroundPixels, image.Height); y < image.Height; y++)
        {
            for (int x = image.Width - Math.Min(MinBackgroundPixels, image.Width); x < image.Width; x++)
            {
                cornerColors.Add(image[x, y]);
            }
        }
        
        // Return the most common corner color (mode)
        return GetMostCommonColor(cornerColors);
    }
    
    /// <summary>
    /// Gets the most common color from a list
    /// </summary>
    private Rgba32 GetMostCommonColor(List<Rgba32> colors)
    {
        if (colors.Count == 0)
            return new Rgba32(255, 255, 255); // Default white
            
        // Group by color and find the most common
        var grouped = colors
            .GroupBy(c => (c.R, c.G, c.B))
            .OrderByDescending(g => g.Count())
            .First();
            
        var dominant = grouped.First();
        return new Rgba32(dominant.R, dominant.G, dominant.B, dominant.A);
    }
    
    /// <summary>
    /// Removes pixels similar to the background color
    /// </summary>
    private void RemoveBackgroundPixels(Image<Rgba32> image, Rgba32 backgroundColor)
    {
        // Use a more sophisticated approach: flood fill from edges
        // but mark pixels as transparent if they're similar to background
        
        var width = image.Width;
        var height = image.Height;
        
        // For each pixel, check if it's similar to background
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = image[x, y];
                
                // Skip if already transparent
                if (pixel.A < 128)
                    continue;
                    
                // Calculate color distance
                var distance = ColorDistance(pixel, backgroundColor);
                
                if (distance < SimilarityThreshold)
                {
                    // Make transparent
                    image[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, 0);
                }
            }
        }
        
        // Second pass: clean up edges using edge-aware processing
        // This helps remove halos around the subject
        CleanUpEdges(image, backgroundColor);
    }
    
    /// <summary>
    /// Cleans up edges to remove halos
    /// </summary>
    private void CleanUpEdges(Image<Rgba32> image, Rgba32 backgroundColor)
    {
        var width = image.Width;
        var height = image.Height;
        var newAlpha = new byte[width * height];
        
        // Copy current alpha values
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newAlpha[y * width + x] = image[x, y].A;
            }
        }
        
        // Apply morphological dilation to expand transparent areas
        // This helps remove halos
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                var idx = y * width + x;
                
                // If this pixel is opaque but neighbors are transparent, make it transparent
                // This is a simple edge cleanup
                if (newAlpha[idx] > 128)
                {
                    bool hasTransparentNeighbor = 
                        newAlpha[idx - 1] < 128 ||
                        newAlpha[idx + 1] < 128 ||
                        newAlpha[idx - width] < 128 ||
                        newAlpha[idx + width] < 128;
                    
                    if (hasTransparentNeighbor)
                    {
                        var pixel = image[x, y];
                        var distance = ColorDistance(pixel, backgroundColor);
                        
                        // If close to background color, make transparent
                        if (distance < SimilarityThreshold * 2)
                        {
                            newAlpha[idx] = 0;
                        }
                    }
                }
            }
        }
        
        // Apply new alpha values
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var idx = y * width + x;
                var pixel = image[x, y];
                image[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, newAlpha[idx]);
            }
        }
    }
    
    /// <summary>
    /// Calculates Euclidean distance between two colors in RGB space
    /// </summary>
    private double ColorDistance(Rgba32 c1, Rgba32 c2)
    {
        var dr = c1.R - c2.R;
        var dg = c1.G - c2.G;
        var db = c1.B - c2.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }
}
