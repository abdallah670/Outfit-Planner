using MediatR;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Social.Requests.Commands;

public class LikeOutfitCommand : IRequest<BaseCommandResponse>
{
    public Guid OutfitId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class UnlikeOutfitCommand : IRequest<BaseCommandResponse>
{
    public Guid OutfitId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
