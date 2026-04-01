using MediatR;
using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

public class GetPostCommentsQuery : IRequest<PostCommentsResponse>
{
    public Guid PostId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
