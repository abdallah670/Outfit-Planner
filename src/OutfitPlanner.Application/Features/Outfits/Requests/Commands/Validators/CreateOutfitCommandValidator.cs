using FluentValidation;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands.Validators;

public class CreateOutfitCommandValidator : AbstractValidator<CreateOutfitCommand>
{
    public CreateOutfitCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name is required")
            .When(x => x.Request != null);

        RuleFor(x => x.Request.Occasion)
            .NotEmpty().WithMessage("Occasion is required")
            .Must(BeValidOccasion).WithMessage("Invalid occasion type")
            .When(x => x.Request != null);

        RuleFor(x => x.Request.Season)
            .NotEmpty().WithMessage("Season is required")
            .Must(BeValidSeason).WithMessage("Invalid season")
            .When(x => x.Request != null);

        RuleFor(x => x.Request.Items)
            .NotNull().WithMessage("Items are required")
            .Must(items => items != null && items.Count >= 1).WithMessage("At least one item is required")
            .When(x => x.Request != null);
    }

    private bool BeValidOccasion(string occasion)
    {
        return Enum.TryParse<OccasionType>(occasion, true, out _);
    }

    private bool BeValidSeason(string season)
    {
        return Enum.TryParse<Season>(season, true, out _);
    }
}
