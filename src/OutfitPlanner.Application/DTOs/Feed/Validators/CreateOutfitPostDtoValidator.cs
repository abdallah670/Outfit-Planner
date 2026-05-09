using FluentValidation;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.DTOs.Feed.Validators;

public class CreateOutfitPostDtoValidator : AbstractValidator<CreateOutfitPostDto>
{
    public CreateOutfitPostDtoValidator()
    {
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
