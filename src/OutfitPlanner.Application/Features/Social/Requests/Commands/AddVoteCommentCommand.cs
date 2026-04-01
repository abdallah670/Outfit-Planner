using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

public class AddVoteCommentCommand : IRequest<BaseCommandResponse>
{
    public Guid VoteId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}
