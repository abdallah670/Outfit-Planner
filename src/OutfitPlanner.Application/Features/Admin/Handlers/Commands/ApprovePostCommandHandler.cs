using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Commands;
using OutfitPlanner.Application;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.Common.Result;
using OutfitPlanner.Domain.Entities;
using FeedPost = OutfitPlanner.Domain.Entities.FeedPost;
using Outfit = OutfitPlanner.Domain.Entities.Outfit;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Commands;

/// <summary>
/// Handler for ApprovePostCommand
/// </summary>
public class ApprovePostCommandHandler : IRequestHandler<ApprovePostCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApprovePostCommandHandler> _logger;

    public ApprovePostCommandHandler(IUnitOfWork unitOfWork, ILogger<ApprovePostCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ApprovePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _unitOfWork.Repository<FeedPost>()
                .GetFirstOrDefaultAsync(p => p.Id == request.PostId);

            if (post == null)
                return Result.Failure("Post not found");

            post.IsApproved = true;
            post.Status = PostStatus.Approved;
            post.ApprovedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to approve post: {ex.Message}");
        }
    }
}

public class RejectPostCommandHandler : IRequestHandler<RejectPostCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectPostCommandHandler> _logger;

    public RejectPostCommandHandler(IUnitOfWork unitOfWork, ILogger<RejectPostCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RejectPostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _unitOfWork.Repository<FeedPost>()
                .GetFirstOrDefaultAsync(p => p.Id == request.PostId);

            if (post == null)
                return Result.Failure("Post not found");

            post.IsApproved = false;
            post.Status = PostStatus.Rejected;
            post.ApprovedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to reject post: {ex.Message}");
        }
    }
}

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePostCommandHandler> _logger;

    public DeletePostCommandHandler(IUnitOfWork unitOfWork, ILogger<DeletePostCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _unitOfWork.Repository<FeedPost>()
                .GetFirstOrDefaultAsync(p => p.Id == request.PostId);

            if (post == null)
                return Result.Failure("Post not found");

            await _unitOfWork.Repository<FeedPost>().DeleteAsync(post);
            await _unitOfWork.CompleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete post: {ex.Message}");
        }
    }
}

public class BulkPostOperationCommandHandler : IRequestHandler<BulkPostOperationCommand, BulkOperationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkPostOperationCommandHandler> _logger;

    public BulkPostOperationCommandHandler(IUnitOfWork unitOfWork, ILogger<BulkPostOperationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BulkOperationResponse> Handle(BulkPostOperationCommand request, CancellationToken cancellationToken)
    {
        var results = new List<BulkOperationResult>();
        var successfulCount = 0;

        foreach (var operation in request.Operations)
        {
            try
            {
                var post = await _unitOfWork.Repository<FeedPost>()
                    .GetFirstOrDefaultAsync(p => p.Id == operation.PostId);

                if (post == null)
                {
                    results.Add(new BulkOperationResult(operation.PostId, false, "Post not found"));
                    continue;
                }

                switch (operation.Type.ToLower())
                {
                    case "approve":
                        post.IsApproved = true;
                        post.Status = PostStatus.Approved;
                        post.ApprovedAt = DateTime.UtcNow;
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.PostId, true, "Post approved"));
                        break;

                    case "reject":
                        post.IsApproved = false;
                        post.Status = PostStatus.Rejected;
                        post.ApprovedAt = DateTime.UtcNow;
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.PostId, true, "Post rejected"));
                        break;

                    case "delete":
                        await _unitOfWork.Repository<FeedPost>().DeleteAsync(post);
                        successfulCount++;
                        results.Add(new BulkOperationResult(operation.PostId, true, "Post deleted"));
                        break;

                    default:
                        results.Add(new BulkOperationResult(operation.PostId, false, "Invalid operation type"));
                        break;
                }
            }
            catch (Exception ex)
            {
                results.Add(new BulkOperationResult(operation.PostId, false, $"Operation failed: {ex.Message}"));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BulkOperationResponse(
            results,
            request.Operations.Count,
            successfulCount,
            request.Operations.Count - successfulCount
        );
    }
}
