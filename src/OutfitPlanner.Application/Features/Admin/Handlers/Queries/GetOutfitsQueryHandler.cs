using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetOutfitsQueryHandler : IRequestHandler<GetOutfitsQuery, PaginatedResult<AdminOutfitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetOutfitsQueryHandler> _logger;

    public GetOutfitsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetOutfitsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AdminOutfitDto>> Handle(GetOutfitsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Outfit>()
            .GetQueryable(include: o => o
                .Include(o => o.CreatedBy)
                .Include(o => o.OutfitImages))
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            query = query.Where(o => 
                o.Name.Contains(request.Filter.Search) ||
                o.Description.Contains(request.Filter.Search) ||
                (o.Tags != null && o.Tags.Any(tag => tag.Contains(request.Filter.Search))));
        }

        if (!string.IsNullOrEmpty(request.Filter.Status))
        {
            if (Enum.TryParse<OutfitStatus>(request.Filter.Status, out var status))
            {
                switch (status)
                {
                    case OutfitStatus.Featured:
                        query = query.Where(o => o.IsFeatured);
                        break;
                    case OutfitStatus.Hidden:
                        query = query.Where(o => !o.IsApproved);
                        break;
                    case OutfitStatus.Rejected:
                        query = query.Where(o => !o.IsApproved && o.ApprovedAt.HasValue);
                        break;
                    default:
                        // For other statuses, we'd need to add status field to Outfits entity
                        break;
                }
            }
        }

        if (request.Filter.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.Filter.StartDate.Value);
        }

        if (request.Filter.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.Filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var outfits = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(o => new AdminOutfitDto(
                o.Id,
                o.CreatedById,
                o.CreatedBy.UserName ?? "Unknown",
                o.Name,
                o.Description,
                o.Tags ?? new List<string>(),
                o.OutfitImages.Select(img => img.ImageUrl).ToList(),
                o.LikesCount,
                o.CommentsCount,
                o.CreatedAt,
                o.IsFeatured,
                o.IsApproved,
                o.FeaturedAt,
                o.FeaturedById,
                o.ApprovedAt,
                o.ApprovedById
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminOutfitDto>
        {
            Data = outfits,
            Total = totalCount,
            Page = request.Filter.Page,
            PageSize = request.Filter.PageSize
        };
    }
}
