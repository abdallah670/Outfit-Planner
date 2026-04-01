using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

public class DeletePostCommentCommandValidator : AbstractValidator<DeletePostCommentCommand>
{
    public DeletePostCommentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("CommentId is required");
    }
}
