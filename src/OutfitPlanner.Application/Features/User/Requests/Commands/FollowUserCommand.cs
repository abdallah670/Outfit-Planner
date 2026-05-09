using MediatR;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class FollowUserCommand : IRequest<BaseCommandResponse>
{
    public string FollowerId { get; set; } = string.Empty;
    public string FollowingId { get; set; } = string.Empty;
}
