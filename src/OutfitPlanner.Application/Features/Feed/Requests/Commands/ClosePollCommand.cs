using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.Feed.Requests.Commands;

public class ClosePollCommand : IRequest<BaseCommandResponse>
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}
