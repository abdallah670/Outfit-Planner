using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

public class UpdatePostCommentCommand : IRequest<BaseCommandResponse>
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
