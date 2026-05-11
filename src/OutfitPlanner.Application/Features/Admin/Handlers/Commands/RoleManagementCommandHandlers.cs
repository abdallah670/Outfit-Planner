using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Domain.Entities;
namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly ILogger<AssignRoleCommandHandler> _logger;

    public AssignRoleCommandHandler(IUnitOfWork unitOfWork, UserManager<Domain.Entities.User> userManager, ILogger<AssignRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning role {Role} to user {UserId}", request.Role, request.UserId);

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure($"User with ID {request.UserId} not found");
        }

        // Remove existing roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        // Add new role
        var result = await _userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to assign role: {errors}");
        }

        // Update user's Role property
        user.Role = request.Role switch
        {
            "Admin" => Domain.Enums.UserRole.Admin,
            "Planner" => Domain.Enums.UserRole.Planner,
            _ => null
        };

        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully assigned role {Role} to user {UserId}", request.Role, request.UserId);
        return Result.Success();
    }
}

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly ILogger<RemoveRoleCommandHandler> _logger;

    public RemoveRoleCommandHandler(IUnitOfWork unitOfWork, UserManager<Domain.Entities.User> userManager, ILogger<RemoveRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing role {Role} from user {UserId}", request.Role, request.UserId);

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure($"User with ID {request.UserId} not found");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, request.Role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to remove role: {errors}");
        }

        // Update user's Role property
        var remainingRoles = await _userManager.GetRolesAsync(user);
        user.Role = remainingRoles.FirstOrDefault() switch
        {
            "Admin" => Domain.Enums.UserRole.Admin,
            "Planner" => Domain.Enums.UserRole.Planner,
            _ => null
        };

        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully removed role {Role} from user {UserId}", request.Role, request.UserId);
        return Result.Success();
    }
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly ILogger<UpdateUserRoleCommandHandler> _logger;

    public UpdateUserRoleCommandHandler(IUnitOfWork unitOfWork, UserManager<Domain.Entities.User> userManager, ILogger<UpdateUserRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user {UserId} role to {NewRole}", request.UserId, request.NewRole);

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure($"User with ID {request.UserId} not found");
        }

        // Remove all existing roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        // Add new role
        var result = await _userManager.AddToRoleAsync(user, request.NewRole);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to update role: {errors}");
        }

        // Update user's Role property
        user.Role = request.NewRole switch
        {
            "Admin" => Domain.Enums.UserRole.Admin,
            "Planner" => Domain.Enums.UserRole.Planner,
            _ => null
        };

        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated user {UserId} role to {NewRole}", request.UserId, request.NewRole);
        return Result.Success();
    }
}
