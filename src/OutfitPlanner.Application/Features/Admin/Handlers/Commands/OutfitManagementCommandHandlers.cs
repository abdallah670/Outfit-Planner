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
using Outfit = OutfitPlanner.Domain.Entities.Outfit;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class DeleteOutfitCommandHandler : IRequestHandler<DeleteOutfitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteOutfitCommandHandler> _logger;

    public DeleteOutfitCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteOutfitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var outfit = await _unitOfWork.Repository<Outfit>()
                .GetFirstOrDefaultAsync(o => o.Id == request.OutfitId, include: o => o.Include(o => o.Items));

            if (outfit == null)
                return Result.Failure("Outfit not found");

            await _unitOfWork.Repository<Outfit>().RemoveAsync(outfit);
            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete outfit: {ex.Message}");
        }
    }
}

public class BulkOutfitOperationCommandHandler : IRequestHandler<BulkOutfitOperationCommand, BulkOperationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkOutfitOperationCommandHandler> _logger;

    public BulkOutfitOperationCommandHandler(IUnitOfWork unitOfWork, ILogger<BulkOutfitOperationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BulkOperationResponse> Handle(BulkOutfitOperationCommand request, CancellationToken cancellationToken)
    {
        var results = new List<BulkOperationResult>();
        var successfulCount = 0;

        foreach (var operation in request.Operations)
        {
            try
            {
                var outfit = await _unitOfWork.Repository<Outfit>()
                    .GetFirstOrDefaultAsync(o => o.Id == operation.OutfitId);

                if (outfit == null)
                {
                    results.Add(new BulkOperationResult(operation.OutfitId, false, "Outfit not found"));
                    continue;
                }

                switch (operation.Type.ToLower())
                {
                    case "delete":
                        await _unitOfWork.Repository<Outfit>().RemoveAsync(outfit);
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Outfit deleted"));
                        break;

                    case "feature":
                    case "unfeature":
                    case "approve":
                    case "reject":
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Operation ignored (Admin only deletes)"));
                        break;

                    default:
                        results.Add(new BulkOperationResult(operation.OutfitId, false, "Invalid operation type"));
                        break;
                }
            }
            catch (Exception ex)
            {
                results.Add(new BulkOperationResult(operation.OutfitId, false, $"Operation failed: {ex.Message}"));
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

