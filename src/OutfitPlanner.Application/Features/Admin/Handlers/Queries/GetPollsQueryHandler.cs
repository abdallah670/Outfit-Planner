using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using Result = OutfitPlanner.Application.Common.Result;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetPollsQueryHandler : IRequestHandler<GetPollsQuery, PaginatedResult<AdminPollDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public GetPollsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPollsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AdminPollDto>> Handle(GetPollsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<ValidationPoll>()
            .GetQueryable(include: p => p.Include(p => p.PollOptions))
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            query = query.Where(p => 
                p.Question.Contains(request.Filter.Search));
        }

        if (!string.IsNullOrEmpty(request.Filter.Status))
        {
            if (Enum.TryParse<PollStatus>(request.Filter.Status, out var status))
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
        
        var polls = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(p => new AdminPollDto(
                p.Id,
                p.CreatedById,
                p.CreatedBy.UserName ?? "Unknown",
                p.Question,
                p.PollOptions.OrderBy(o => o.Order).Select(o => o.Text).ToList(),
                p.PollOptions.OrderBy(o => o.Order).Select(o => o.Votes).ToList(),
                p.TotalVotes,
                p.CreatedAt,
                p.EndsAt,
                p.Status,
                p.IsFeatured,
                p.FeaturedAt,
                p.FeaturedById
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminPollDto>
        {
            Data = polls,
            Total = totalCount,
            Page = request.Filter.Page,
            PageSize = request.Filter.PageSize
        };
    }
}
