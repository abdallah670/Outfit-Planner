using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

/// <summary>
/// Command to vote on a poll
/// </summary>
public class VoteOnPollCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid PollId { get; set; }
    public CastVoteDto Request { get; set; } = new();
}
