using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

/// <summary>
/// Validator for CreatePollPostCommand
/// </summary>
public class CreatePollCommandValidator : AbstractValidator<CreatePollPostCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question is required")
            .MaximumLength(500)
            .WithMessage("Question cannot exceed 500 characters");

        RuleFor(x => x.ExpiresAt)
            .NotEmpty()
            .WithMessage("Expiration date is required")
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future");

        RuleFor(x => x.Options)
            .NotNull()
            .WithMessage("Options are required")
            .Must(x => x != null && x.Count >= 2 && x.Count <= 4)
            .WithMessage("Between 2 and 4 options are required");
    }
}
