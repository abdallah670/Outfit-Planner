using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<GetRolesQueryHandler> _logger;

    public GetRolesQueryHandler(IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager, ILogger<GetRolesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all roles");

        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var userCount = await _unitOfWork.Repository<Domain.Entities.User>()
                .Get(u => u.Role.ToString() == role.Name)
                .CountAsync(cancellationToken);

            roleDtos.Add(new RoleDto(
                role.Id,
                role.Name!,
                GetRoleDescription(role.Name!),
                userCount
            ));
        }

        return roleDtos;
    }

    private string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            "Admin" => "Full administrative access to all system features",
            "Planner" => "Access to outfit planning and social features",
            _ => "Standard user role"
        };
    }
}

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<UserRoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserRolesQueryHandler> _logger;

    public GetUserRolesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserRolesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<UserRoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all user roles");

        var users = await _unitOfWork.Repository<Domain.Entities.User>()
            .Get(u => u.Role != null)
            .ToListAsync(cancellationToken);

        var userRoleDtos = users.Select(user => new UserRoleDto(
            user.Id,
            user.UserName!,
            user.Email!,
            user.Role!.ToString(),
            user.CreatedAt.DateTime
        )).ToList();

        return userRoleDtos;
    }
}

public class GetRoleManagementQueryHandler : IRequestHandler<GetRoleManagementQuery, RoleManagementDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<GetRoleManagementQueryHandler> _logger;

    public GetRoleManagementQueryHandler(IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager, ILogger<GetRoleManagementQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<RoleManagementDto> Handle(GetRoleManagementQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting role management data");

        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        var users = await _unitOfWork.Repository<Domain.Entities.User>()
            .Get(u => u.Role != null)
            .ToListAsync(cancellationToken);

        var roleDtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var userCount = users.Count(u => u.Role.ToString() == role.Name);
            roleDtos.Add(new RoleDto(
                role.Id,
                role.Name!,
                GetRoleDescription(role.Name!),
                userCount
            ));
        }

        var userRoleDtos = users.Select(user => new UserRoleDto(
            user.Id,
            user.UserName!,
            user.Email!,
            user.Role!.ToString(),
            user.CreatedAt.DateTime
        )).ToList();

        return new RoleManagementDto(
            roleDtos,
            userRoleDtos,
            users.Count,
            users.Count(u => u.Role.ToString() == "Admin"),
            users.Count(u => u.Role.ToString() == "Planner")
        );
    }

    private string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            "Admin" => "Full administrative access to all system features",
            "Planner" => "Access to outfit planning and social features",
            _ => "Standard user role"
        };
    }
}
