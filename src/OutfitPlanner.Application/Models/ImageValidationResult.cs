namespace OutfitPlanner.Application.Contracts.Infrastructure.Models;

/// <summary>
/// Result of image validation.
/// </summary>
public record ImageValidationResult
{
    /// <summary>
    /// Indicates if the image is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ImageValidationResult Valid() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static ImageValidationResult Invalid(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}