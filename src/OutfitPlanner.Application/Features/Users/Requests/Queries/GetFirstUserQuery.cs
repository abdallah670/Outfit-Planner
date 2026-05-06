using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Users.Requests.Queries;

public class GetFirstUserQuery : IRequest<OutfitPlanner.Domain.Entities.User?>
{
}

public class GetFirstUserQueryHandler : IRequestHandler<GetFirstUserQuery, OutfitPlanner.Domain.Entities.User?>
{
    private readonly UserManager<OutfitPlanner.Domain.Entities.User> _userManager;

    public GetFirstUserQueryHandler(UserManager<OutfitPlanner.Domain.Entities.User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<OutfitPlanner.Domain.Entities.User?> Handle(GetFirstUserQuery request, CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .OrderBy(u => u.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
