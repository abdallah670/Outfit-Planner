using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Features.Admin.DTOs;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedResult<AuditLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAuditLogsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<AuditLog>().GetQueryable();
        
        // Apply user filter
        if (request.Filter.UserId.HasValue)
        {
            var userIdString = request.Filter.UserId.Value.ToString();
            query = query.Where(a => a.UserId == userIdString);
        }
        
        // Apply action filter
        if (!string.IsNullOrEmpty(request.Filter.Action))
        {
            query = query.Where(a => a.Action.Contains(request.Filter.Action));
        }
        
        // Apply date range filter
        if (request.Filter.StartDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.Filter.StartDate.Value);
        }
        
        if (request.Filter.EndDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.Filter.EndDate.Value);
        }
        
        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(a => new AuditLogDto(a.Id, a.UserName, a.Action, a.EntityType, a.Timestamp))
            .ToListAsync();
        
        return new PaginatedResult<AuditLogDto>(logs, total, request.Filter.Page, request.Filter.PageSize);
    }
}
