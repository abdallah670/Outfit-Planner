using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.Features.User.Requests.Queries;

namespace OutfitPlanner.Application.Features.User.Handlers.Queries;

public class GetConnectedAccountsQueryHandler : IRequestHandler<GetConnectedAccountsQuery, ConnectedAccountsDto>
{
    private readonly IUserRepository _userRepository;

    public GetConnectedAccountsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ConnectedAccountsDto> Handle(GetConnectedAccountsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            return new ConnectedAccountsDto();
        }

        return new ConnectedAccountsDto
        {
            Provider = user.Provider,
            ProviderId = user.ProviderId
        };
    }
}
