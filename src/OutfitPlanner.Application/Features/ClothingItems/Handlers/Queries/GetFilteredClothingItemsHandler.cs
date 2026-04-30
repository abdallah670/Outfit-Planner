using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Common;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries;

public class GetFilteredClothingItemsHandler : IRequestHandler<GetFilteredClothingItemsRequest, PagedResult<ClothingItemListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFilteredClothingItemsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ClothingItemListDto>> Handle(GetFilteredClothingItemsRequest request, CancellationToken cancellationToken)
    {
        // Get queryable from repository - filtered by user and active status
        var query = _unitOfWork.ClothingItems.Get(
            filter: c => c.UserId == request.UserId && c.IsActive,
            include: null);

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categoryLower = request.Category.ToLower();
            query = query.Where(c => c.Category.ToLower() == categoryLower);
        }

        // Apply color filter
        if (!string.IsNullOrWhiteSpace(request.Color))
        {
            var colorLower = request.Color.ToLower();
            query = query.Where(c => c.PrimaryColor.ToLower().Contains(colorLower));
        }

        // Apply search query
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchLower = request.SearchQuery.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.Brand.ToLower().Contains(searchLower) ||
                c.Category.ToLower().Contains(searchLower));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var skip = (request.Page - 1) * request.PageSize;
        var items = await query
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(c => new ClothingItemListDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                ThumbnailUrl = c.ThumbnailUrl,
                Category = c.Category,
                Type = c.Type.ToString(),
                PrimaryColor = c.PrimaryColor,
                WearCount = c.WearCount,
                Brand = c.Brand
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ClothingItemListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
