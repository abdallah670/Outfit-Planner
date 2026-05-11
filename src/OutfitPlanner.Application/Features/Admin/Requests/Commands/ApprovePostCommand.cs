using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;


public record DeletePostCommand(Guid PostId) : IRequest<Result>;

public record BulkPostOperationCommand(List<PostOperation> Operations) : IRequest<BulkOperationResponse>;

public record PostOperation(
    Guid PostId,
    string Type, // "approve", "reject", "delete"
    string? Reason = null
);

public record BulkOperationResponse(
    List<BulkOperationResult> Results,
    int TotalOperations,
    int SuccessfulOperations,
    int FailedOperations
);

public record BulkOperationResult(
    Guid PostId,
    bool Success,
    string? Message = null
);
