using MediatR;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Feed.Requests.Queries;

/// <summary>
/// Query to get voters for a poll with optional option filter
/// </summary>
public class GetPollVotersQuery : IRequest<IEnumerable<(Vote Vote, string VoterName, string? VoterAvatarUrl)>>
{
    public Guid PollId { get; set; }
    public Guid? OptionId { get; set; }
}
