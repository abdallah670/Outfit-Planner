namespace OutfitPlanner.Application.DTOs.Admin;

public record RoleDto(
    string Id,
    string Name,
    string Description,
    int UserCount
);

public record UserRoleDto(
    string UserId,
    string UserName,
    string Email,
    string Role,
    DateTime AssignedAt
);

public record RoleAssignmentRequest(
    string UserId,
    string Role
);

public record RoleManagementDto(
    List<RoleDto> Roles,
    List<UserRoleDto> UserRoles,
    int TotalUsers,
    int AdminCount,
    int PlannerCount
);
