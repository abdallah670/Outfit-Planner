using FluentValidation;
using OutfitPlanner.Application.Features.Social.Requests.Commands;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands.Validators;

/// <summary>
/// Validator for CreatePollCommand
/// </summary>
public class CreatePollCommandValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Request.Question)
            .NotEmpty()
            .WithMessage("Question is required")
            .MaximumLength(500)
            .WithMessage("Question cannot exceed 500 characters");

        RuleFor(x => x.Request.ExpiresAt)
            .NotEmpty()
            .WithMessage("Expiration date is required")
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future");

        RuleFor(x => x.Request.Options)
            .NotNull()
            .WithMessage("Options are required")
            .Must(x => x != null && x.Count >= 2)
            .WithMessage("At least 2 options are required");

        RuleForEach(x => x.Request.Options).SetValidator(new CreatePollOptionDtoValidator());
    }
}

/// <summary>
/// Validator for CreatePollOptionDto
/// </summary>
public class CreatePollOptionDtoValidator : AbstractValidator<DTOs.Social.CreatePollOptionDto>
{
    public CreatePollOptionDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be non-negative");
    }
}
