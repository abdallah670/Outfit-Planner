using MediatR;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs;

namespace OutfitPlanner.Application.Features.Admin.Requests.Commands;

public record ClosePollCommand(Guid PollId, string Reason) : IRequest<Result>;

public record FeaturePollCommand(Guid PollId) : IRequest<Result>;

public record UnfeaturePollCommand(Guid PollId) : IRequest<Result>;

public record DeletePollCommand(Guid PollId) : IRequest<Result>;

public record BulkPollOperationCommand(List<PollOperation> Operations) : IRequest<BulkOperationResponse>;

public record PollOperation(
    Guid PollId,
    string Type, // "close", "feature", "unfeature", "delete"
    string? Reason = null
);
