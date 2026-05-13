using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common;
using OutfitPlanner.Application.DTOs.Admin;
using OutfitPlanner.Application.Features.Admin.Requests.Queries;
using OutfitPlanner.Application;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;
using FeedPost = OutfitPlanner.Domain.Entities.FeedPost;

namespace OutfitPlanner.Application.Features.Admin.Handlers.Queries;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, PaginatedResult<AdminPostDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPostsQueryHandler> _logger;

    public GetPostsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPostsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<AdminPostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<FeedPost>().GetQueryable(include: p => p
            .Include(p => p.User)
            .Include(p => p.Outfit)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.ClothingItem)
            .Include(p => p.Poll)
                .ThenInclude(poll => poll.Options));

        // Apply filters
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            query = query.Where(p => 
                (p.Caption != null && p.Caption.Contains(request.Filter.Search)) ||
                (p.Outfit != null && p.Outfit.Name.Contains(request.Filter.Search)) ||
                (p.Poll != null && p.Poll.Question.Contains(request.Filter.Search)));
        }

        if (!string.IsNullOrEmpty(request.Filter.ContentType))
        {
            if (Enum.TryParse<OutfitPlanner.Domain.Enums.PostType>(request.Filter.ContentType, out var type))
            {
                query = query.Where(p => p.PostType == type);
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
                p.UserId,
                p.User != null ? p.User.UserName ?? "Unknown" : "Unknown",
                p.Caption,
                p.Tags ?? new List<string>(),
                p.LikesCount,
                p.CommentsCount,
                p.CreatedAt.DateTime,
                p.PostType,
                // Outfit
                p.OutfitId,
                p.Outfit != null ? p.Outfit.Name : null,
                p.Outfit != null ? p.Outfit.ImageUrl : null,
                p.Outfit != null ? p.Outfit.Items.Select(i => i.ClothingItem.ImageUrl).ToList() : null,
                // Poll
                p.PollId,
                p.Poll != null ? p.Poll.Question : null,
                p.Poll != null ? p.Poll.Options.OrderBy(o => o.DisplayOrder).Select(o => o.Id).ToList() : null,
                p.Poll != null ? p.Poll.Options.OrderBy(o => o.DisplayOrder).Select(o => o.Votes.Count).ToList() : null,
                p.Poll != null ? p.Poll.TotalVotes : null,
                p.Poll != null ? p.Poll.ExpiresAt.DateTime : (DateTime?)null
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
