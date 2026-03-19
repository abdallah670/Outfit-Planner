using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetConnectedAccountsQueryHandler : IRequestHandler<GetConnectedAccountsQuery, List<ConnectedAccountDto>>
{
    private readonly UserManager<OutfitPlanner.Domain.Entities.User> _userManager;

    public GetConnectedAccountsQueryHandler(UserManager<OutfitPlanner.Domain.Entities.User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<ConnectedAccountDto>> Handle(GetConnectedAccountsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        if (user == null)
        {
            return new List<ConnectedAccountDto>();
        }

        var externalLogins = await _userManager.GetLoginsAsync(user);
        
        if (externalLogins == null || !externalLogins.Any())
        {
            return new List<ConnectedAccountDto>();
        }

        return externalLogins.Select(login => new ConnectedAccountDto
        {
            Provider = login.LoginProvider,
            ProviderId = login.ProviderKey,
            Email = user.Email ?? string.Empty,
            ConnectedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
        }).ToList();
    }
}
