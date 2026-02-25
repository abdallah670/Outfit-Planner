using FluentValidation;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands.Validators;

public class DeleteClothingItemCommandValidator : AbstractValidator<DeleteClothingItemCommand>
{
    public DeleteClothingItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Clothing item ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
