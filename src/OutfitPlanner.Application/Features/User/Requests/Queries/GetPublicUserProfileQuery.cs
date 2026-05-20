using MediatR;
using OutfitPlanner.Application.DTOs.User;

namespace OutfitPlanner.Application.Features.User.Requests.Queries;

public class GetPublicUserProfileQuery : IRequest<PublicUserProfileDto?>
{
    public string UserId { get; set; } = string.Empty;
    public string RequesterId { get; set; } = string.Empty;
}
