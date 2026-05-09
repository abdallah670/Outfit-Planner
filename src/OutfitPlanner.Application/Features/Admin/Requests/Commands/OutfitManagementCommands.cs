using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record DeleteOutfitCommand(Guid OutfitId) : IRequest<Result>;

public record BulkOutfitOperationCommand(List<OutfitOperation> Operations) : IRequest<BulkOperationResponse>;

public record OutfitOperation(
    Guid OutfitId,
    string Type, // "delete"
    string? Reason = null
);
