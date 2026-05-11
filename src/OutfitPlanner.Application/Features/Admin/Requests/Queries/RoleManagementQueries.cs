using MediatR;
using OutfitPlanner.Application.DTOs.Admin;

namespace OutfitPlanner.Application.Features.Admin.Requests.Queries;

public record GetRolesQuery : IRequest<List<RoleDto>>;

public record GetUserRolesQuery : IRequest<List<UserRoleDto>>;

public record GetRoleManagementQuery : IRequest<RoleManagementDto>;
