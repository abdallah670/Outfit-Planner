using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries.Validators;

public class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(x => x.Cursor)
            .Empty()
            .When(x => x.Cursor != null)
            .WithMessage("Cursor cannot be empty when provided");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100");

        RuleFor(x => x.SortBy)
            .Must(x => x == "popular" || x == "recent")
            .WithMessage("SortBy must be either 'popular' or 'recent'");

        RuleFor(x => x.Visibility)
            .Must(x => x == "Private" || x == "Followers" || x == "Public")
            .WithMessage("Visibility must be 'Private', 'Followers', or 'Public'");
    }
}

public class GetFeedPostByIdQueryValidator : AbstractValidator<GetFeedPostByIdQuery>
{
    public GetFeedPostByIdQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required");
    }
}

public class GetPostCommentsQueryValidator : AbstractValidator<GetPostCommentsQuery>
{
    public GetPostCommentsQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required");

        RuleFor(x => x.Cursor)
            .Empty()
            .When(x => x.Cursor != null)
            .WithMessage("Cursor cannot be empty when provided");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("PageSize cannot exceed 50");
    }
}
