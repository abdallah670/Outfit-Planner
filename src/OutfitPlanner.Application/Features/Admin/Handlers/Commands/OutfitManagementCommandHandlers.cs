using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;


public class FeatureOutfitCommandHandler : IRequestHandler<FeatureOutfitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FeatureOutfitCommandHandler> _logger;

    public FeatureOutfitCommandHandler(IUnitOfWork unitOfWork, ILogger<FeatureOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(FeatureOutfitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var outfit = await _unitOfWork.Repository<Outfit>()
                .GetFirstOrDefaultAsync(o => o.Id == request.OutfitId);

            if (outfit == null)
                return Result.Failure("Outfit not found");

            outfit.IsFeatured = true;
            outfit.FeaturedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to feature outfit: {ex.Message}");
        }
    }
}

public class UnfeatureOutfitCommandHandler : IRequestHandler<UnfeatureOutfitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnfeatureOutfitCommandHandler> _logger;

    public UnfeatureOutfitCommandHandler(IUnitOfWork unitOfWork, ILogger<UnfeatureOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UnfeatureOutfitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var outfit = await _unitOfWork.Repository<Outfit>()
                .GetFirstOrDefaultAsync(o => o.Id == request.OutfitId);

            if (outfit == null)
                return Result.Failure("Outfit not found");

            outfit.IsFeatured = false;
            outfit.FeaturedAt = null;

            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to unfeature outfit: {ex.Message}");
        }
    }
}

public class ApproveOutfitCommandHandler : IRequestHandler<ApproveOutfitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveOutfitCommandHandler> _logger;

    public ApproveOutfitCommandHandler(IUnitOfWork unitOfWork, ILogger<ApproveOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ApproveOutfitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var outfit = await _unitOfWork.Repository<Outfit>()
                .GetFirstOrDefaultAsync(o => o.Id == request.OutfitId);

            if (outfit == null)
                return Result.Failure("Outfit not found");

            outfit.IsApproved = true;
            outfit.ApprovedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to approve outfit: {ex.Message}");
        }
    }
}

public class RejectOutfitCommandHandler : IRequestHandler<RejectOutfitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectOutfitCommandHandler> _logger;

    public RejectOutfitCommandHandler(IUnitOfWork unitOfWork, ILogger<RejectOutfitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RejectOutfitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var outfit = await _unitOfWork.Repository<Outfit>()
                .GetFirstOrDefaultAsync(o => o.Id == request.OutfitId);

            if (outfit == null)
                return Result.Failure("Outfit not found");

            outfit.IsApproved = false;
            outfit.ApprovedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to reject outfit: {ex.Message}");
        }
    }
}

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
                .GetFirstOrDefaultAsync(o => o.Id == request.OutfitId, include: o => o.Include(o => o.OutfitImages));

            if (outfit == null)
                return Result.Failure("Outfit not found");

            await _unitOfWork.Repository<Outfit>().DeleteAsync(outfit);
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
                var outfit = await _context.Outfits
                    .FirstOrDefaultAsync(o => o.Id == operation.OutfitId, cancellationToken);

                if (outfit == null)
                {
                    results.Add(new BulkOperationResult(operation.OutfitId, false, "Outfit not found"));
                    continue;
                }

                switch (operation.Type.ToLower())
                {
                    case "feature":
                        outfit.IsFeatured = true;
                        outfit.FeaturedAt = DateTime.UtcNow;
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Outfit featured"));
                        break;

                    case "unfeature":
                        outfit.IsFeatured = false;
                        outfit.FeaturedAt = null;
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Outfit unfeatured"));
                        break;

                    case "approve":
                        outfit.IsApproved = true;
                        outfit.ApprovedAt = DateTime.UtcNow;
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Outfit approved"));
                        break;

                    case "reject":
                        outfit.IsApproved = false;
                        outfit.ApprovedAt = DateTime.UtcNow;
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Outfit rejected"));
                        break;

                    case "delete":
                        await _unitOfWork.Repository<Outfit>().DeleteAsync(outfit);
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.OutfitId, true, "Outfit deleted"));
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

        await _context.SaveChangesAsync(cancellationToken);

        return new BulkOperationResponse(
            results,
            request.Operations.Count,
            successfulCount,
            request.Operations.Count - successfulCount
        );
    }
}

