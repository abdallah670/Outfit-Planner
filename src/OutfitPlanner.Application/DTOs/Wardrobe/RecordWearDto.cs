using System.ComponentModel.DataAnnotations;

namespace OutfitPlanner.Application.DTOs.Wardrobe;

/// <summary>
/// Request DTO for recording a wear event for a clothing item
/// </summary>
public class RecordWearDto
{
    /// <summary>
    /// The ID of the clothing item that was worn
    /// </summary>
    [Required(ErrorMessage = "Clothing item ID is required")]
    public Guid ClothingItemId { get; set; }

    /// <summary>
    /// The date and time when the item was worn
    /// </summary>
    [Required(ErrorMessage = "Worn date is required")]
    public DateTimeOffset WornAt { get; set; }

    /// <summary>
    /// How long the item was worn in minutes (optional)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number")]
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Weather conditions when the item was worn (optional)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Weather condition cannot exceed 100 characters")]
    public string? WeatherCondition { get; set; }

    /// <summary>
    /// User's rating of the wear experience (1-5, optional)
    /// </summary>
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    /// <summary>
    /// Additional notes about the wear event (optional)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}