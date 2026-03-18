using FluentValidation;

namespace OutfitPlanner.Application.Features.Notifications.Requests.Commands.Validators;

public class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
