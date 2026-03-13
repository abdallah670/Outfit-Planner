using FluentValidation;
using OutfitPlanner.Application.DTOs.Calendar.Validators;

namespace OutfitPlanner.Application.Features.Calendar.Requests.Commands.Validators;

public class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required")
            .SetValidator(new CreateCalendarEventRequestValidator());
    }
}
