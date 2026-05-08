using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record FeatureOutfitCommand(Guid OutfitId) : IRequest<Result>;

public record UnfeatureOutfitCommand(Guid OutfitId) : IRequest<Result>;

public record ApproveOutfitCommand(Guid OutfitId) : IRequest<Result>;

public record RejectOutfitCommand(Guid OutfitId, string Reason) : IRequest<Result>;

public record DeleteOutfitCommand(Guid OutfitId) : IRequest<Result>;

public record BulkOutfitOperationCommand(List<OutfitOperation> Operations) : IRequest<BulkOperationResponse>;

public record OutfitOperation(
    Guid OutfitId,
    string Type, // "feature", "unfeature", "approve", "reject", "delete"
    string? Reason = null
);
