using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetProfilePictureQueryHandler : IRequestHandler<GetProfilePictureQuery, ProfilePictureDto>
{
    private readonly UserManager<OutfitPlanner.Domain.Entities.User> _userManager;

    public GetProfilePictureQueryHandler(UserManager<OutfitPlanner.Domain.Entities.User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ProfilePictureDto> Handle(GetProfilePictureQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            throw new NotFoundException(nameof(OutfitPlanner.Domain.Entities.User), request.UserId);
        }

        return new ProfilePictureDto
        {
            UserId = user.Id,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }
}
