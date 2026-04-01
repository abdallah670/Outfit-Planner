using MediatR;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

public class GetFeedPostByIdQuery : IRequest<FeedPostDto?>
{
    public Guid PostId { get; set; }
    public string? UserId { get; set; }
}
