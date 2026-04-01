using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

public class AddPostReactionCommandValidator : AbstractValidator<AddPostReactionCommand>
{
    public AddPostReactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required");

        RuleFor(x => x.ReactionType)
            .NotEmpty()
            .WithMessage("ReactionType is required")
            .Must(x => x == "Heart" || x == "Fire" || x == "Clap")
            .WithMessage("Invalid reaction type");
    }
}
