using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

public class ReactToVoteCommand : IRequest<BaseCommandResponse>
{
    public Guid VoteId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ReactionRequest Request { get; set; } = null!;
}
