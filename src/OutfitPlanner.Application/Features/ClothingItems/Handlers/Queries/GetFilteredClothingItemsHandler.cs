using MediatR;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Responses;
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

    // Map color name → hex codes stored in PrimaryColor field (includes leading '#')
    private static string[] GetHexCodesForColor(string colorName)
    {
        var color = colorName.ToLower();
        return color switch
        {
            "black" => new[] { "#000000", "#1f2937", "#111827", "#0f172a" },
            "white" => new[] { "#ffffff", "#f3f4f6", "#f9fafb", "#f5f5f5", "#e5e7eb", "#fafafa" },
            "gray" => new[] { "#808080", "#9ca3af", "#d1d5db", "#6b7280" },
            "blue" => new[] { "#3b82f6", "#2563eb", "#1d4ed8", "#60a5fa", "#93c5fd", "#dbeafe", "#0ea5e9" },
            "red" => new[] { "#ef4444", "#dc2626", "#b91c1c", "#f87171", "#fecaca", "#f43f5e" },
            "green" => new[] { "#22c55e", "#16a34a", "#15803d", "#86efac", "#4ade80", "#bbf7d0", "#9caf88", "#84cc16" },
            "pink" => new[] { "#ec4899", "#db2777", "#be185d", "#f472b6", "#fce7f3", "#f8b4c4" },
            "beige" => new[] { "#d6b78a", "#d6c4a0", "#c9a66b", "#b8956f", "#a1887f", "#e7d9c4", "#f5deb3" },
            "yellow" => new[] { "#fbbf24", "#f59e0b", "#d97706", "#fef08a", "#fef9c3", "#fde047" },
            "orange" => new[] { "#fb923c", "#f97316", "#ea580c", "#fed7aa", "#ff8a00" },
            "purple" => new[] { "#8b5cf6", "#7c3aed", "#6d28d9", "#a78bfa", "#d8b4fe", "#c084fc" },
            "navy" => new[] { "#1e3a5f", "#1e40af", "#1e3a8a", "#172554", "#0f172a" },
            _ => Array.Empty<string>(),
        };
    }

    public async Task<PagedResult<ClothingItemListDto>> Handle(GetFilteredClothingItemsRequest request, CancellationToken cancellationToken)
    {
        // Get queryable from repository - filtered by user and active status
        var query = _unitOfWork.ClothingItems.Get(
            filter: c => c.UserId == request.UserId && c.IsActive,
            include: null);

        // Apply type filter (maps to ClothingType enum)
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (Enum.TryParse<ClothingType>(request.Type, true, out var typeEnum))
            {
                query = query.Where(c => c.Type == typeEnum);
            }
        }

        // Apply category filter (e.g. "Casual", "Formal", "Sport" – stored as free-text string)
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categoryLower = request.Category.ToLower();
            query = query.Where(c => c.Category.ToLower() == categoryLower);
        }

        // Apply color filter – match PrimaryColor hex against known hex codes for the color name
        if (!string.IsNullOrWhiteSpace(request.Color))
        {
            var hexList = GetHexCodesForColor(request.Color);
            if (hexList.Length > 0)
            {
                // EF Core will translate Contains into an IN clause
                query = query.Where(c => hexList.Contains(c.PrimaryColor));
            }
            else
            {
                // Unknown color name → no results
                query = query.Where(c => false);
            }
        }

        // Apply condition filter
        if (!string.IsNullOrWhiteSpace(request.Condition))
        {
            var conditionLower = request.Condition.ToLower();
            query = query.Where(c => c.Condition.ToLower() == conditionLower);
        }

        // Apply fabric filter
        if (!string.IsNullOrWhiteSpace(request.Fabric))
        {
            var fabricLower = request.Fabric.ToLower();
            query = query.Where(c => c.Fabric.ToString().ToLower() == fabricLower);
        }

        // Apply size filter
        if (!string.IsNullOrWhiteSpace(request.Size))
        {
            var sizeLower = request.Size.ToLower();
            query = query.Where(c => c.Size.ToLower() == sizeLower);
        }

        // Apply price range filter
        if (request.MinPrice.HasValue)
        {
            query = query.Where(c => c.PurchasePrice.Amount >= request.MinPrice.Value);
        }
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(c => c.PurchasePrice.Amount <= request.MaxPrice.Value);
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
                Brand = c.Brand,
                Condition = c.Condition,
                PurchasePrice = c.PurchasePrice.Amount,
                LastWorn = c.LastWorn,
                CreatedAt = c.CreatedAt
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
