using FluentValidation;
using OutfitPlanner.Application.DTOs.Notification;
using OutfitPlanner.Application.DTOs.Notification.Validators;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Commands.Validators;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required")
            .SetValidator(new CreateNotificationRequestValidator());
    }
}
