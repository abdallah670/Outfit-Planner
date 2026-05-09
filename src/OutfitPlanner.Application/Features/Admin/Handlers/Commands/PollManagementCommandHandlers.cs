using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application;
using static OutfitPlanner.Application.Common.Result;
using OutfitPlanner.Domain.Entities;
using ValidationPoll = OutfitPlanner.Domain.Entities.ValidationPoll;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class ClosePollCommandHandler : IRequestHandler<ClosePollCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClosePollCommandHandler> _logger;

    public ClosePollCommandHandler(IUnitOfWork unitOfWork, ILogger<ClosePollCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ClosePollCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var poll = await _unitOfWork.Repository<ValidationPoll>()
                .GetFirstOrDefaultAsync(p => p.Id == request.PollId);

            if (poll == null)
                return Result.Failure("Poll not found");

            poll.Status = PollStatus.Closed;
            poll.ExpiresAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to close poll: {ex.Message}");
        }
    }
}

public class FeaturePollCommandHandler : IRequestHandler<FeaturePollCommand, Result>
{
    public async Task<Result> Handle(FeaturePollCommand request, CancellationToken cancellationToken)
    {
        return Result.Success("Feature operation ignored (Admin only deletes)");
    }
}

public class UnfeaturePollCommandHandler : IRequestHandler<UnfeaturePollCommand, Result>
{
    public async Task<Result> Handle(UnfeaturePollCommand request, CancellationToken cancellationToken)
    {
        return Result.Success("Unfeature operation ignored (Admin only deletes)");
    }
}

public class DeletePollCommandHandler : IRequestHandler<DeletePollCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePollCommandHandler> _logger;

    public DeletePollCommandHandler(IUnitOfWork unitOfWork, ILogger<DeletePollCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePollCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var poll = await _unitOfWork.Repository<ValidationPoll>()
                .GetFirstOrDefaultAsync(p => p.Id == request.PollId, include: p => p.Include(p => p.Options));

            if (poll == null)
                return Result.Failure("Poll not found");

            await _unitOfWork.Repository<ValidationPoll>().RemoveAsync(poll);
            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete poll: {ex.Message}");
        }
    }
}

public class BulkPollOperationCommandHandler : IRequestHandler<BulkPollOperationCommand, BulkOperationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkPollOperationCommandHandler> _logger;

    public BulkPollOperationCommandHandler(IUnitOfWork unitOfWork, ILogger<BulkPollOperationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BulkOperationResponse> Handle(BulkPollOperationCommand request, CancellationToken cancellationToken)
    {
        var results = new List<BulkOperationResult>();
        var successfulCount = 0;

        foreach (var operation in request.Operations)
        {
            try
            {
                var poll = await _unitOfWork.Repository<ValidationPoll>()
                    .GetFirstOrDefaultAsync(p => p.Id == operation.PollId);

                if (poll == null)
                {
                    results.Add(new BulkOperationResult(operation.PollId, false, "Poll not found"));
                    continue;
                }

                switch (operation.Type.ToLower())
                {
                    case "delete":
                        await _unitOfWork.Repository<ValidationPoll>().RemoveAsync(poll);
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.PollId, true, "Poll deleted"));
                        break;

                    default:
                        // Admin only deletes, but we return success to not block bulk flow
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.PollId, true, "Operation ignored (Admin only deletes)"));
                        break;
                }
            }
            catch (Exception ex)
            {
                results.Add(new BulkOperationResult(operation.PollId, false, $"Operation failed: {ex.Message}"));
            }
        }

        await _unitOfWork.CompleteAsync();

        return new BulkOperationResponse(
            results,
            request.Operations.Count,
            successfulCount,
            request.Operations.Count - successfulCount
        );
    }
}
