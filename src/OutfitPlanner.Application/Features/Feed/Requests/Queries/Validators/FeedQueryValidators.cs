using FluentValidation;
using OutfitPlanner.Application.Features.Feed.Requests.Queries;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries.Validators;

public class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(x => x.Cursor)
            .NotEmpty()
            .WithMessage("Cursor is required");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100");

        RuleFor(x => x.SortBy)
            .Must(x => x == "popular" || x == "recent")
            .WithMessage("SortBy must be either 'popular' or 'recent'");

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage("Invalid visibility value");
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

        RuleFor(x=>x.Cursor)
            .NotEmpty()
            .WithMessage("Cursor is required");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("PageSize cannot exceed 50");
    }
}
