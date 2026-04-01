using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

public class LikeVoteCommentCommand : IRequest<BaseCommandResponse>
{
    public Guid CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
