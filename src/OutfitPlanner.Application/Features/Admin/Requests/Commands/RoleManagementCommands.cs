using MediatR;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Common;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record AssignRoleCommand(string UserId, string Role) : IRequest<Result>;

public record RemoveRoleCommand(string UserId, string Role) : IRequest<Result>;

public record UpdateUserRoleCommand(string UserId, string NewRole) : IRequest<Result>;
