using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.OutfitPoll.Requests.Commands;

public class LikeOutfitCommand : IRequest<OutfitVoteResultDto>
{
    public Guid OutfitId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? Rating { get; set; }
}
