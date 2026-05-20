using FluentValidation;

namespace OutfitPlanner.Application.DTOs.Wardrobe.Validators;

/// <summary>
/// Validator for RecordWearRequest
/// </summary>
public class RecordWearRequestValidator : AbstractValidator<RecordWearDto>
{
    public RecordWearRequestValidator()
    {
        RuleFor(x => x.ClothingItemId)
            .NotEmpty()
            .WithMessage("Clothing item ID is required");

        RuleFor(x => x.WornAt)
            .NotEmpty()
            .WithMessage("Worn date is required")
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Worn date cannot be in the future");

        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Duration must be a positive number")
            .When(x => x.DurationMinutes.HasValue);

        RuleFor(x => x.WeatherCondition)
            .MaximumLength(100)
            .WithMessage("Weather condition cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.WeatherCondition));

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
