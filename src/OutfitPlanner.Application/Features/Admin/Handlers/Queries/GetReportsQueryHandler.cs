using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, PaginatedResult<ContentReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetReportsQueryHandler> _logger;

    public GetReportsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetReportsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<ContentReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<ContentReport>()
            .GetQueryable(include: r => r.Include(r => r.ReporterUser).Include(r => r.TargetUser))
            .AsQueryable();
        
        // Apply status filter
        if (request.Filter.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Filter.Status.Value);
        }
        
        // Apply content type filter
        if (!string.IsNullOrEmpty(request.Filter.ContentType))
        {
            query = query.Where(r => r.ContentType == request.Filter.ContentType);
        }
        
        var total = await query.CountAsync(cancellationToken);
        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(r => new ContentReportDto(
                r.Id,
                r.ReporterUser?.UserName,
                r.TargetUserId,
                r.ContentType,
                r.Reason,
                r.Status,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);
        
        return new PaginatedResult<ContentReportDto>(reports, total, request.Filter.Page, request.Filter.PageSize);
    }
}
