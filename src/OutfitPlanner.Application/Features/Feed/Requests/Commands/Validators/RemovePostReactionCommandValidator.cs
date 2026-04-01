using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

public class RemovePostReactionCommandValidator : AbstractValidator<RemovePostReactionCommand>
{
    public RemovePostReactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required");
    }
}
