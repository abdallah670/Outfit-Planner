using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.Features.Outfits.Requests.Queries;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Outfits.Handlers.Queries;

public class GetFilteredOutfitsHandler : IRequestHandler<GetFilteredOutfitsRequest, PagedResult<OutfitDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFilteredOutfitsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<OutfitDto>> Handle(GetFilteredOutfitsRequest request, CancellationToken cancellationToken)
    {
        // Get queryable with include for Items and ClothingItem
        var query = _unitOfWork.Outfits.Get(
            filter: o => o.UserId == request.UserId && o.Status != OutfitStatus.Deleted,
            include: q => q.Include(o => o.Items).ThenInclude(oi => oi.ClothingItem));

        // Apply occasion filter
        if (!string.IsNullOrWhiteSpace(request.Occasion))
        {
            if (Enum.TryParse<OccasionType>(request.Occasion, true, out var occasion))
            {
                query = query.Where(o => o.Occasion == occasion);
            }
        }

        // Apply season filter
        if (!string.IsNullOrWhiteSpace(request.Season))
        {
            if (Enum.TryParse<Season>(request.Season, true, out var season))
            {
                query = query.Where(o => o.Season == season);
            }
        }

        // Apply search query
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchLower = request.SearchQuery.ToLower();
            query = query.Where(o => o.Name.ToLower().Contains(searchLower));
        }

        // Apply sorting (before pagination, after filtering)
        IOrderedQueryable<Outfit> orderedQuery = request.SortBy?.ToLower() switch
        {
            "mostworn" => query.OrderByDescending(o => o.TimesWorn),
            "name" => query.OrderBy(o => o.Name),
            "recent" => query.OrderByDescending(o => o.CreatedAt),
            _ => query.OrderByDescending(o => o.CreatedAt) // default: recent
        };

        // Get total count before pagination
        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        // Apply pagination
        var skip = (request.Page - 1) * request.PageSize;
        var outfits = await orderedQuery
            .Skip(skip)
            .Take(request.PageSize)
            .Select(o => new OutfitDto
            {
                Id = o.Id,
                Name = o.Name,
                UserId = o.UserId,
                Occasion = o.Occasion.ToString(),
                WeatherCondition = o.WeatherCondition,
                Season = o.Season.ToString(),
                ComfortRating = o.ComfortRating,
                StyleRating = o.StyleRating,
                LastWorn = o.LastWorn,
                TimesWorn = o.TimesWorn,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                ImageUrl = o.ImageUrl,
                Items = o.Items.Select(oi => new OutfitItemDto
                {
                    Id = oi.Id,
                    ClothingItemId = oi.ClothingItemId,
                    ClothingItemName = oi.ClothingItem != null ? oi.ClothingItem.Name : "",
                    ClothingItemImageUrl = oi.ClothingItem != null ? oi.ClothingItem.ImageUrl : "",
                    ClothingItemType = oi.ClothingItem != null ? oi.ClothingItem.Type :ClothingType.Top,
                    ClothingItemCategory = oi.ClothingItem != null ? oi.ClothingItem.Category : "",
                    Role = oi.Role.ToString(),
                    LayeringOrder = oi.LayeringOrder,
                    IsEssential = oi.IsEssential
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OutfitDto>
        {
            Items = outfits,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
