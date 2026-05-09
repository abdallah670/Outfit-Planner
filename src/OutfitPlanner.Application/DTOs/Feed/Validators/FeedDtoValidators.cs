using FluentValidation;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.DTOs.Feed.Validators;

public class CreatePollPostDtoValidator : AbstractValidator<CreatePollPostDto>
{
    public CreatePollPostDtoValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question is required")
            .MaximumLength(500)
            .WithMessage("Question cannot exceed 500 characters");

        RuleFor(x => x.OutfitIds)
            .NotNull()
            .WithMessage("OutfitIds is required")
            .Must(x => x.Count >= 2)
            .WithMessage("At least 2 outfits are required")
            .Must(x => x.Count <= 4)
            .WithMessage("Maximum 4 outfits allowed");

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

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters");
    }
}
