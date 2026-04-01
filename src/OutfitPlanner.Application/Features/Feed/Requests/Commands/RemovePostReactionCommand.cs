using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

public class RemovePostReactionCommand : IRequest<BaseCommandResponse>
{
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
