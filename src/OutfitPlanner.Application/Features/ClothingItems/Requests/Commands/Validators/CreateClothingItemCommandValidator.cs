using FluentValidation;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.DTOs.Wardrobe.Validators;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;

public class CreateClothingItemCommandValidator : AbstractValidator<CreateClothingItemCommand>
{
    public CreateClothingItemCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required")
            .SetValidator(new CreateClothingItemRequestValidator());
    }
}
