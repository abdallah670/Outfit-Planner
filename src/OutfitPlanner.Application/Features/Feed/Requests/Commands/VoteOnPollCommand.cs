using MediatR;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

/// <summary>
/// Command to vote on a poll
/// </summary>
public class VoteOnPollCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public Guid PollId { get; set; }
    public CastVoteDto Request { get; set; } = new();
}

public class UnVoteOnPollCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public unCastVoteDto Request { get; set; } = new();
}
