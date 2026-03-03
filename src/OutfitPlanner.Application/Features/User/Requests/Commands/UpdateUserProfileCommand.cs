using MediatR;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Requests.Commands;

public class UpdateUserProfileCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public UpdateUserProfileDto Request { get; set; } = null!;
}
