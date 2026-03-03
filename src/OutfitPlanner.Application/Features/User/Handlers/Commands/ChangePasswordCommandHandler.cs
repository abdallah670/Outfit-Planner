using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, BaseCommandResponse>
{
    private readonly UserManager<Domain.Entities.User> _userManager;

    public ChangePasswordCommandHandler(UserManager<Domain.Entities.User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<BaseCommandResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
        }

        // Check if new password and confirm password match
        if (request.Request.NewPassword != request.Request.ConfirmPassword)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "New password and confirmation password do not match",
                Errors = new List<string> { "Passwords do not match" }
            };
        }

        var result = await _userManager.ChangePasswordAsync(
            user, 
            request.Request.CurrentPassword, 
            request.Request.NewPassword);

        if (!result.Succeeded)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "Failed to change password",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Password changed successfully"
        };
    }
}
