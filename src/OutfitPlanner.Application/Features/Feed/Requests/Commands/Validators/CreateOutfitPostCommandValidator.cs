using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

public class CreateOutfitPostCommandValidator : AbstractValidator<CreateOutfitPostCommand>
{
    public CreateOutfitPostCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.OutfitId)
            .NotEmpty()
            .WithMessage("OutfitId is required");

        RuleFor(x => x.Caption)
            .MaximumLength(500)
            .WithMessage("Caption cannot exceed 500 characters");

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage("Invalid visibility value");
    }
}
