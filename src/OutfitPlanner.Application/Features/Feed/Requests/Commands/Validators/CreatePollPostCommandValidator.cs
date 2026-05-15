using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

public class CreatePollPostCommandValidator : AbstractValidator<CreatePollPostCommand>
{
    public CreatePollPostCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question is required")
            .MaximumLength(500)
            .WithMessage("Question cannot exceed 500 characters");

        RuleFor(x => x.Options)
            .NotNull()
            .WithMessage("Options is required")
            .Must(x => x.Count >= 2)
            .WithMessage("At least 2 options are required")
            .Must(x => x.Count <= 4)
            .WithMessage("Maximum 4 options allowed");

        RuleFor(x => x.ExpiresAt)
            .NotEmpty()
            .WithMessage("ExpiresAt is required")
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future");

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage("Invalid visibility value");
    }
}
