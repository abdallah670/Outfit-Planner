using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using System.Collections.Generic;

namespace OutfitPlanner.Application.Features.Social.Requests.Queries;

public class GetVoteCommentsQuery : IRequest<List<VoteCommentDto>>
{
    public Guid VoteId { get; set; }
    public int MaxDepth { get; set; } = 3;
}
