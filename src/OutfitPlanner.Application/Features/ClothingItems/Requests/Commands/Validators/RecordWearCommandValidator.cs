using FluentValidation;
using OutfitPlanner.Application.DTOs.Wardrobe.Validators;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;

public class RecordWearCommandValidator : AbstractValidator<RecordWearCommand>
{
    public RecordWearCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request is required");

        RuleFor(x => x.Request)
            .SetValidator(new RecordWearRequestValidator());
    }
}
