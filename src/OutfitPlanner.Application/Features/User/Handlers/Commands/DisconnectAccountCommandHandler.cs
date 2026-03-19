using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class DisconnectAccountCommandHandler : IRequestHandler<DisconnectAccountCommand, BaseCommandResponse>
{
    private readonly UserManager<OutfitPlanner.Domain.Entities.User> _userManager;
    private readonly ILogger<DisconnectAccountCommandHandler> _logger;

    public DisconnectAccountCommandHandler(
        UserManager<OutfitPlanner.Domain.Entities.User> userManager,
        ILogger<DisconnectAccountCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<BaseCommandResponse> Handle(DisconnectAccountCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();

        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            var externalLogins = await _userManager.GetLoginsAsync(user);
            var loginToRemove = externalLogins.FirstOrDefault(l => 
                l.LoginProvider.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
            
            if (loginToRemove == null)
            {
                response.Success = false;
                response.Message = $"No {request.Provider} account is connected";
                return response;
            }

            var result = await _userManager.RemoveLoginAsync(user, loginToRemove.LoginProvider, loginToRemove.ProviderKey);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                response.Success = false;
                response.Message = $"Failed to disconnect {request.Provider} account: {errors}";
                return response;
            }

            response.Success = true;
            response.Message = $"{request.Provider} account disconnected successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting {Provider} account for user {UserId}", 
                request.Provider, request.UserId);
            response.Success = false;
            response.Message = $"Failed to disconnect {request.Provider} account";
        }

        return response;
    }
}
