using FluentValidation;
using OutfitPlanner.Application.DTOs.Wardrobe.Validators;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;

public class UpdateClothingItemCommandValidator : AbstractValidator<UpdateClothingItemCommand>
{
    public UpdateClothingItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Clothing item ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required")
            .SetValidator(new UpdateClothingItemRequestValidator());
    }
}
