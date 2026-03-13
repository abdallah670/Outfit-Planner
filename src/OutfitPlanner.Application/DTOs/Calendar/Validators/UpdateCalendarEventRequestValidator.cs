using FluentValidation;

namespace OutfitPlanner.Application.DTOs.Calendar.Validators;

public class UpdateCalendarEventRequestValidator : AbstractValidator<UpdateCalendarEventRequest>
{
    public UpdateCalendarEventRequestValidator()
    {
        RuleFor(x => x.WornAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Worn at date cannot be in the future")
            .When(x => x.WornAt.HasValue);

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be positive")
            .When(x => x.DurationMinutes.HasValue);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.WeatherCondition)
            .MaximumLength(100).WithMessage("Weather condition must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.WeatherCondition));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
