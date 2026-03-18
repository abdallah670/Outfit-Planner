using FluentValidation;

namespace OutfitPlanner.Application.DTOs.Notification.Validators;

public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid notification type");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters");

        RuleFor(x => x.ActionUrl)
            .MaximumLength(500).WithMessage("Action URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ActionUrl));
    }
}
