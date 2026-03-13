using FluentValidation;

namespace OutfitPlanner.Application.DTOs.Calendar.Validators;

public class CreateCalendarEventRequestValidator : AbstractValidator<CreateCalendarEventRequest>
{
    public CreateCalendarEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.EventDate)
            .NotEmpty().WithMessage("Event date is required");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("Invalid event type");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
