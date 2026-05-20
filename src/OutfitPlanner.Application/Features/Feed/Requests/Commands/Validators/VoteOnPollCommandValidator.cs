using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Commands;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands.Validators;

/// <summary>
/// Validator for VoteOnPollCommand
/// </summary>
public class VoteOnPollCommandValidator : AbstractValidator<VoteOnPollCommand>
{
    public VoteOnPollCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.PollId)
            .NotEmpty()
            .WithMessage("Poll ID is required");

        RuleFor(x => x.Request.OptionId)
            .NotEmpty()
            .WithMessage("Option ID is required");
    }
}
