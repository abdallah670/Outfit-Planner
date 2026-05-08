using MediatR;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record CreateAuditLogCommand(string Action, string Details, Guid UserId, string? UserName = null, string? EntityType = null, Guid? EntityId = null, string? OldValues = null, string? NewValues = null, string? IpAddress = null) : IRequest;
