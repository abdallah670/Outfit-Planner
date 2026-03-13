using MediatR;
using Microsoft.AspNetCore.Identity;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.User.Requests.Commands;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Domain.Entities;
using ApplicationUser = OutfitPlanner.Domain.Entities.User;

namespace OutfitPlanner.Application.Features.User.Handlers.Commands;

public class UpdateEmailCommandHandler : IRequestHandler<UpdateEmailCommand, BaseCommandResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateEmailCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<BaseCommandResponse> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(ApplicationUser), request.UserId);
        }

        // Verify current password before changing email
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Request.CurrentPassword);
        
        if (!isPasswordValid)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "Current password is incorrect"
            };
        }

        // Check if the new email is already in use
        var existingUser = await _userManager.FindByEmailAsync(request.Request.NewEmail);
        
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = "Email is already in use"
            };
        }

        // Generate email change token
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.Request.NewEmail);
        
        // Change email
        var result = await _userManager.ChangeEmailAsync(user, request.Request.NewEmail, token);
        
        if (!result.Succeeded)
        {
            return new BaseCommandResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new BaseCommandResponse
        {
            Success = true,
            Message = "Email updated successfully",
            Id = Guid.Parse(user.Id)
        };
    }
}