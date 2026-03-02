using FluentValidation;
using OutfitPlanner.Application.Features.Outfits.Requests.Commands;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands.Validators;

public class UpdateOutfitCommandValidator : AbstractValidator<UpdateOutfitCommand>
{
    public UpdateOutfitCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Request.Occasion)
            .Must(BeValidOccasionOrNull).WithMessage("Invalid occasion type")
            .When(x => x.Request != null && x.Request.Occasion != null);

        RuleFor(x => x.Request.Season)
            .Must(BeValidSeasonOrNull).WithMessage("Invalid season")
            .When(x => x.Request != null && x.Request.Season != null);

        RuleFor(x => x.Request.Items)
            .Must(items => items == null || items.Count >= 1).WithMessage("At least one item is required when updating items")
            .When(x => x.Request?.Items != null);
    }

    private bool BeValidOccasionOrNull(string? occasion)
    {
        if (string.IsNullOrEmpty(occasion)) return true;
        return Enum.TryParse<OccasionType>(occasion, true, out _);
    }

    private bool BeValidSeasonOrNull(string? season)
    {
        if (string.IsNullOrEmpty(season)) return true;
        return Enum.TryParse<Season>(season, true, out _);
    }
}
