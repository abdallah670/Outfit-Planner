using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using OutfitPlanner.Application.Common.Interfaces.Persistence;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, PaginatedResult<AdminPostDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public GetPostsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPostsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AdminPostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<FeedPost>()
            .GetQueryable(include: p => p.Include(p => p.CreatedBy));

        // Apply filters
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            query = query.Where(p => 
                p.Title.Contains(request.Filter.Search) ||
                p.Content.Contains(request.Filter.Search));
        }

        if (!string.IsNullOrEmpty(request.Filter.Status))
        {
            if (Enum.TryParse<PostStatus>(request.Filter.Status, out var status))
            {
                query = query.Where(p => p.Status == status);
            }
        }

        if (request.Filter.StartDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= request.Filter.StartDate.Value);
        }

        if (request.Filter.EndDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= request.Filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(p => new AdminPostDto(
                p.Id,
                p.CreatedById,
                p.CreatedBy.UserName ?? "Unknown",
                p.Title,
                p.Content,
                p.Tags ?? new List<string>(),
                p.LikesCount,
                p.CommentsCount,
                p.CreatedAt,
                p.IsApproved,
                p.Status,
                p.ApprovedAt,
                p.ApprovedById
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminPostDto>
        {
            Data = posts,
            Total = totalCount,
            Page = request.Filter.Page,
            PageSize = request.Filter.PageSize
        };
    }
}
